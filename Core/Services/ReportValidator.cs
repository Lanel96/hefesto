using Microsoft.Extensions.Logging;
using OracleReportGenerator.Core.Interfaces;
using OracleReportGenerator.Core.Models;

namespace OracleReportGenerator.Core.Services;

public class ReportValidator : IReportValidator
{
    private readonly ILogger<ReportValidator> _logger;

    public ReportValidator(ILogger<ReportValidator> logger)
    {
        _logger = logger;
    }

    public ValidationResult Validate(List<ExamRecord> records, ReportConfig config)
    {
        var result = new ValidationResult
        {
            OriginalCount = records.Count
        };

        if (records.Count == 0)
        {
            result.IsValid = false;
            result.Errors.Add("No se obtuvieron registros de la base de datos");
            return result;
        }

        var examNumbers = config.ExamNumbers.Distinct().ToList();

        // Validar CODI no nulo
        var nullCodi = records.Where(r => string.IsNullOrWhiteSpace(r.Codi)).ToList();
        if (nullCodi.Any())
        {
            result.Warnings.Add($"Se encontraron {nullCodi.Count} registros con CODI nulo/vacío - se excluirán");
            records = records.Where(r => !string.IsNullOrWhiteSpace(r.Codi)).ToList();
        }

        // Validar fechas
        var invalidDates = records.Where(r => 
            examNumbers.Any(e => GetPerfDate(r, e)?.Year < 2000)).ToList();
        
        if (invalidDates.Any())
        {
            result.Warnings.Add($"Se encontraron {invalidDates.Count} registros con fechas inválidas");
        }

        // Agrupar por CODI y validar que tengan TODOS los exámenes configurados
        var codisWithAllExams = records
            .GroupBy(r => r.Codi)
            .Where(g => examNumbers.All(e => g.Any(r => r.Exam == e)))
            .Select(g => g.Key)
            .ToHashSet();

        var codisWithoutAll = records
            .GroupBy(r => r.Codi)
            .Where(g => !examNumbers.All(e => g.Any(r => r.Exam == e)))
            .Select(g => g.Key)
            .ToList();

        if (codisWithoutAll.Any())
        {
            result.Warnings.Add($"{codisWithoutAll.Count} CODI(s) no tienen todos los exámenes requeridos ({string.Join(", ", examNumbers)}) - se excluirán: {string.Join(", ", codisWithoutAll.Take(10))}{(codisWithoutAll.Count > 10 ? "..." : "")}");
        }

        // Filtrar solo CODI válidos
        var validRecords = records.Where(r => codisWithAllExams.Contains(r.Codi)).ToList();
        
        result.ValidCount = validRecords.Count;
        result.UniqueCodis = validRecords.Select(r => r.Codi).Distinct().Count();
        result.IsValid = result.ValidCount > 0;

        _logger.LogInformation("Validación: Original={Original}, Válidos={Valid}, CODI únicos={Unique} | Exámenes requeridos: {Exams}", 
            result.OriginalCount, result.ValidCount, result.UniqueCodis, string.Join(", ", examNumbers));

        if (!result.IsValid)
        {
            result.Errors.Add("No hay registros válidos después de la validación");
        }

        return result;
    }

    private static DateTime? GetPerfDate(ExamRecord record, int examNumber)
    {
        return record.PerfDates.TryGetValue(examNumber, out var date) ? date : null;
    }
}