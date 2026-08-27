namespace OracleReportGenerator.Core.Interfaces;

using OracleReportGenerator.Core.Models;

public interface IOracleRepository
{
    Task<List<ExamRecord>> GetExamRecordsAsync(ReportConfig config, CancellationToken cancellationToken = default);
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
}

public interface IReportValidator
{
    ValidationResult Validate(List<ExamRecord> records, ReportConfig config);
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public int OriginalCount { get; set; }
    public int ValidCount { get; set; }
    public int UniqueCodis { get; set; }
}

public interface IReportGenerator
{
    Task GenerateReportsAsync(List<PatientReport> reports, string outputDirectory, CancellationToken cancellationToken = default);
    Task GenerateGlobalReportAsync(List<PatientReport> reports, List<int> examNumbers, string outputFile, CancellationToken cancellationToken = default);
}

public interface IReportGrouper
{
    List<PatientReport> GroupByCodi(List<ExamRecord> records, DateTime fechaIni, DateTime fechaFin);
}