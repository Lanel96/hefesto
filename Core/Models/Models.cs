namespace OracleReportGenerator.Core.Models;

public class ExamRecord
{
    public string Codi { get; set; } = string.Empty;
    public int Exam { get; set; }
    public Dictionary<int, DateTime?> PerfDates { get; set; } = new();
    public Dictionary<int, DateTime?> ExamDates { get; set; } = new();
}

public class PatientReport
{
    public string Codi { get; set; } = string.Empty;
    public List<ExamRecord> Exams { get; set; } = new();
    public DateTime FechaIni { get; set; }
    public DateTime FechaFin { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    public int TotalExams => Exams.Count;
}

public class OracleConnectionConfig
{
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
    public int ConnectionTimeout { get; set; } = 30;
    
    public string ConnectionString => 
        $"User Id={User};Password={Password};Data Source={DataSource};Connection Timeout={ConnectionTimeout};";
}

public class ReportConfig
{
    public DateTime FechaIni { get; set; } = new(2025, 7, 1);
    public DateTime FechaFin { get; set; } = new(2025, 8, 1);
    public string OutputDirectory { get; set; } = "reports";
    public string SqlFilePath { get; set; } = "query.sql";
    public List<int> ExamNumbers { get; set; } = new() { 5, 360 };
    
    public string ExamNumbersSql => string.Join(",", ExamNumbers);
    public string ExamNumbersInClause => string.Join(" OR d.exam = ", ExamNumbers.Select(e => $":exam_{e}"));
    public string PerfInClause => string.Join(" OR d.perf = ", ExamNumbers.Select(e => $":perf_{e}"));
}