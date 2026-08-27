using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;
using OracleReportGenerator.Core.Interfaces;
using OracleReportGenerator.Core.Models;
using System.Linq;
using System.Data;

namespace OracleReportGenerator.Core.Services;

public class OracleRepository : IOracleRepository
{
    private readonly OracleConnectionConfig _config;
    private readonly ILogger<OracleRepository> _logger;

    public OracleRepository(IOptions<OracleConnectionConfig> config, ILogger<OracleRepository> logger)
    {
        _config = config.Value;
        _logger = logger;
    }

    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new OracleConnection(_config.ConnectionString);
            await connection.OpenAsync(cancellationToken);
            _logger.LogInformation("Conexión a Oracle exitosa");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error conectando a Oracle: {Message}", ex.Message);
            return false;
        }
    }

    public async Task<List<ExamRecord>> GetExamRecordsAsync(ReportConfig config, CancellationToken cancellationToken = default)
    {
        var records = new List<ExamRecord>();
        var sql = await File.ReadAllTextAsync(config.SqlFilePath, cancellationToken);

        // Remover duplicados de ExamNumbers para evitar índices de columna inválidos
        var uniqueExamNumbers = config.ExamNumbers.Distinct().ToList();

        if (uniqueExamNumbers.Count < 2)
        {
            _logger.LogError("Se requieren al menos 2 números de examen únicos para la comparación");
            return records;
        }

        using var connection = new OracleConnection(_config.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.BindByName = true;
        command.CommandTimeout = _config.ConnectionTimeout;

        command.Parameters.Add("fecha_ini", OracleDbType.Date, config.FechaIni, ParameterDirection.Input);
        command.Parameters.Add("fecha_fin", OracleDbType.Date, config.FechaFin, ParameterDirection.Input);

        for (int i = 0; i < config.ExamNumbers.Count; i++)
        {
            var examNum = config.ExamNumbers[i];
            var paramName = $"exam_{i + 1}";
            command.Parameters.Add(paramName, OracleDbType.Int32, examNum, ParameterDirection.Input);
            _logger.LogDebug("Parámetro {ParamName} = {Value}", paramName, examNum);
        }

        _logger.LogInformation("Ejecutando query para período {FechaIni} - {FechaFin} | Exámenes: {Exams}", 
            config.FechaIni.ToShortDateString(), config.FechaFin.ToShortDateString(), string.Join(", ", config.ExamNumbers));

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var record = new ExamRecord
            {
                Codi = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                Exam = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
            };

            for (int i = 0; i < uniqueExamNumbers.Count; i++)
            {
                var perfIdx = 2 + i;
                var examIdx = 2 + uniqueExamNumbers.Count + i;
                
                var perfDate = reader.IsDBNull(perfIdx) ? (DateTime?)null : reader.GetDateTime(perfIdx);
                var examDate = reader.IsDBNull(examIdx) ? (DateTime?)null : reader.GetDateTime(examIdx);

                SetExamDateProperty(record, uniqueExamNumbers[i], perfDate, examDate);
            }

            records.Add(record);
        }

        _logger.LogInformation("Registros obtenidos: {Count}", records.Count);
        return records;
    }

private static void SetExamDateProperty(ExamRecord record, int examNumber, DateTime? perfDate, DateTime? examDate)
    {
        record.PerfDates[examNumber] = perfDate;
        record.ExamDates[examNumber] = examDate;
    }
}