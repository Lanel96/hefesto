using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OracleReportGenerator.Core.Interfaces;
using OracleReportGenerator.Core.Models;
using OracleReportGenerator.Core.Services;
using OracleReportGenerator.Core.Reports;
using System.IO;

namespace OracleReportGenerator.App;

internal class Program
{
    static async Task<int> Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // Configuración
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args);

        builder.Services.Configure<OracleConnectionConfig>(builder.Configuration.GetSection("Oracle"));
        builder.Services.Configure<ReportConfig>(builder.Configuration.GetSection("Report"));

        // Logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(LogLevel.Information);

        // Servicios
        builder.Services.AddSingleton<IOracleRepository, OracleRepository>();
        builder.Services.AddSingleton<IReportValidator, ReportValidator>();
        builder.Services.AddSingleton<IReportGrouper, ReportGrouper>();
        builder.Services.AddSingleton<IReportGenerator, QuestPdfReportGenerator>();

        // App principal
        builder.Services.AddHostedService<ReportGeneratorApp>();

        var host = builder.Build();
        await host.RunAsync();
        return 0;
    }
}

public class ReportGeneratorApp : IHostedService
{
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<ReportGeneratorApp> _logger;
    private readonly IOracleRepository _repository;
    private readonly IReportValidator _validator;
    private readonly IReportGrouper _grouper;
    private readonly IReportGenerator _generator;
    private readonly OracleConnectionConfig _oracleConfig;
    private readonly ReportConfig _reportConfig;

    public ReportGeneratorApp(
        IHostApplicationLifetime lifetime,
        ILogger<ReportGeneratorApp> logger,
        IOracleRepository repository,
        IReportValidator validator,
        IReportGrouper grouper,
        IReportGenerator generator,
        IOptions<OracleConnectionConfig> oracleConfig,
        IOptions<ReportConfig> reportConfig)
    {
        _lifetime = lifetime;
        _logger = logger;
        _repository = repository;
        _validator = validator;
        _grouper = grouper;
        _generator = generator;
        _oracleConfig = oracleConfig.Value;
        _reportConfig = reportConfig.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("=== Generador de Reportes Oracle (.NET 8) ===");
            _logger.LogInformation("Período: {FechaIni} a {FechaFin}", _reportConfig.FechaIni.ToShortDateString(), _reportConfig.FechaFin.ToShortDateString());

            // Validar configuración
            if (string.IsNullOrWhiteSpace(_oracleConfig.User) || string.IsNullOrWhiteSpace(_oracleConfig.DataSource))
            {
                _logger.LogError("Configuración de Oracle incompleta. Revise appsettings.json");
                _lifetime.StopApplication();
                return;
            }

            // Probar conexión
            _logger.LogInformation("Probando conexión a Oracle...");
            var connected = await _repository.TestConnectionAsync(cancellationToken);
            if (!connected)
            {
                _logger.LogError("No se pudo conectar a Oracle. Verifique credenciales y red.");
                _lifetime.StopApplication();
                return;
            }

            // Obtener datos
            _logger.LogInformation("Obteniendo datos...");
            var records = await _repository.GetExamRecordsAsync(_reportConfig, cancellationToken);

            // Validar
            _logger.LogInformation("Validando datos... | Exámenes: {Exams}", string.Join(", ", _reportConfig.ExamNumbers));
            var validation = _validator.Validate(records, _reportConfig);
            
            if (!validation.IsValid)
            {
                _logger.LogError("Validación fallida: {Errors}", string.Join("; ", validation.Errors));
                _lifetime.StopApplication();
                return;
            }

            if (validation.Warnings.Any())
            {
                foreach (var w in validation.Warnings)
                    _logger.LogWarning("Validación: {Warning}", w);
            }

            _logger.LogInformation("Registros válidos: {Valid}/{Original} | CODI únicos: {Unique}", 
                validation.ValidCount, validation.OriginalCount, validation.UniqueCodis);

            // Agrupar por CODI
            _logger.LogInformation("Agrupando por CODI...");
            var reports = _grouper.GroupByCodi(records.Where(r => !string.IsNullOrWhiteSpace(r.Codi)).ToList(), 
                _reportConfig.FechaIni, _reportConfig.FechaFin);

            _logger.LogInformation("Reportes a generar: {Count}", reports.Count);

            // Generar PDFs
            _logger.LogInformation("Generando reporte global PDF único...");
            var globalOutputPath = Path.Combine(_reportConfig.OutputDirectory, "reporte_global.pdf");
            var uniqueExamNumbers = _reportConfig.ExamNumbers.Distinct().ToList();
            await _generator.GenerateGlobalReportAsync(reports, uniqueExamNumbers, globalOutputPath, cancellationToken);

            _logger.LogInformation("✓ Completado. Reporte global en: {Path}", Path.GetFullPath(globalOutputPath));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fatal: {Message}", ex.Message);
        }
        finally
        {
            _lifetime.StopApplication();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}