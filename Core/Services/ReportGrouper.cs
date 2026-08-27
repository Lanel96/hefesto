using OracleReportGenerator.Core.Interfaces;
using OracleReportGenerator.Core.Models;

namespace OracleReportGenerator.Core.Services;

public class ReportGrouper : IReportGrouper
{
    public List<PatientReport> GroupByCodi(List<ExamRecord> records, DateTime fechaIni, DateTime fechaFin)
    {
        return records
            .GroupBy(r => r.Codi)
            .Select(g => new PatientReport
            {
                Codi = g.Key,
                Exams = g.OrderBy(r => r.Exam).ToList(),
                FechaIni = fechaIni,
                FechaFin = fechaFin,
                GeneratedAt = DateTime.Now
            })
            .OrderBy(p => p.Codi)
            .ToList();
    }
}