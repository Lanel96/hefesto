using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using OracleReportGenerator.Core.Interfaces;
using OracleReportGenerator.Core.Models;
using System.IO;

namespace OracleReportGenerator.Core.Reports;

public class QuestPdfReportGenerator : IReportGenerator
{
    private readonly ILogger<QuestPdfReportGenerator> _logger;

    public QuestPdfReportGenerator(ILogger<QuestPdfReportGenerator> logger)
    {
        _logger = logger;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task GenerateReportsAsync(List<PatientReport> reports, string outputDirectory, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(outputDirectory);
        
        _logger.LogInformation("Generando {Count} reportes PDF en {Directory}", reports.Count, outputDirectory);

        foreach (var report in reports)
        {
            await GenerateSingleReportAsync(report, outputDirectory, cancellationToken);
        }
        
        _logger.LogInformation("Todos los reportes generados exitosamente");
    }

    private async Task GenerateSingleReportAsync(PatientReport report, string outputDirectory, CancellationToken cancellationToken)
    {
        var fileName = $"reporte_{SanitizeFileName(report.Codi)}.pdf";
        var filePath = Path.Combine(outputDirectory, fileName);

        var document = new PatientReportDocument(report);
        document.GeneratePdf(filePath);
        
        _logger.LogDebug("Generado: {FilePath}", filePath);
        await Task.CompletedTask;
    }

    public async Task GenerateGlobalReportAsync(List<PatientReport> reports, List<int> examNumbers, string outputFile, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generando reporte global único con {Count} pacientes", reports.Count);
        
        var document = new GlobalPatientReportDocument(reports, examNumbers);
        document.GeneratePdf(outputFile);
        
        _logger.LogInformation("Reporte global generado: {FilePath}", outputFile);
        await Task.CompletedTask;
    }

    private static string SanitizeFileName(string fileName)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(c, '_');
        }
        return fileName;
    }

    public class GlobalPatientReportDocument : IDocument
    {
        private readonly List<PatientReport> _reports;
        private readonly List<int> _examNumbers;

        public GlobalPatientReportDocument(List<PatientReport> reports, List<int> examNumbers)
        {
            _reports = reports;
            _examNumbers = examNumbers;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(40);
                    page.Size(PageSizes.A4.Landscape());
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Helvetica"));
                    
                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().Element(ComposeFooter);
                });
        }

        void ComposeHeader(IContainer container)
        {
            var examLabels = string.Join(", ", _examNumbers.Select(e => $"Examen {e}"));
            container
                .PaddingBottom(15)
                .BorderBottom(1.5f)
                .BorderColor(Colors.Blue.Darken3)
                .Column(col =>
                {
                    col.Item().AlignCenter().Text("REPORTE GLOBAL DE EXÁMENES DE LABORATORIO")
                        .FontSize(16).Bold().FontColor(Colors.Blue.Darken3);
                    
                    col.Item().AlignCenter().Text($"Período: {_reports.First().FechaIni:dd/MM/yyyy} – {_reports.First().FechaFin:dd/MM/yyyy}  |  Exámenes: {examLabels}")
                        .FontSize(10).SemiBold();
                    
                    col.Item().AlignCenter().Text($"Total de pacientes: {_reports.Count} | Total de registros: {_reports.Sum(r => r.TotalExams)}")
                        .FontSize(9).FontColor(Colors.Grey.Medium);
                });
        }

        void ComposeContent(IContainer container)
        {
            container.PaddingVertical(10).Column(col =>
            {
                col.Item().Element(ComposeFlatTable);
                col.Item().PaddingTop(15).Element(ComposeNote);
            });
        }

        void ComposeFlatTable(IContainer container)
        {
            var headerStyle = TextStyle.Default.SemiBold().FontSize(7);
            var cellStyle = TextStyle.Default.FontSize(7);

            container.Table(table =>
            {
                // Columnas: Paciente + Exámen + (Perf N + Exam N) * examNumbers.Count
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1.5f); // Paciente
                    columns.RelativeColumn(0.8f); // Exámen
                    foreach (var exam in _examNumbers)
                    {
                        columns.RelativeColumn(1.2f); // Perf N
                        columns.RelativeColumn(1.2f); // Exam N
                    }
                });

                // Encabezados dinámicos
                table.Header(header =>
                {
                    header.Cell().Border(0.5f).BorderColor(Colors.Grey.Medium).Background(Colors.Blue.Medium).Padding(4).Text("Paciente").Style(headerStyle).FontColor(Colors.White);
                    header.Cell().Border(0.5f).BorderColor(Colors.Grey.Medium).Background(Colors.Blue.Medium).Padding(4).Text("Exám.").Style(headerStyle).FontColor(Colors.White);
                    foreach (var exam in _examNumbers)
                    {
                        header.Cell().Border(0.5f).BorderColor(Colors.Grey.Medium).Background(Colors.Blue.Medium).Padding(4).Text($"Perf {exam}\n(Fecha Auto)").Style(headerStyle).FontColor(Colors.White);
                        header.Cell().Border(0.5f).BorderColor(Colors.Grey.Medium).Background(Colors.Blue.Medium).Padding(4).Text($"Exam {exam}\n(Fecha Auto)").Style(headerStyle).FontColor(Colors.White);
                    }
                });

                // Filas de datos - cada registro es una fila
                int rowIdx = 0;
                foreach (var report in _reports)
                {
                    foreach (var exam in report.Exams)
                    {
                        var bg = rowIdx % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;

                        table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Background(bg).Padding(4).Text(report.Codi).Style(cellStyle).SemiBold();
                        table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Background(bg).Padding(4).Text(exam.Exam.ToString()).Style(cellStyle);

                        foreach (var examNum in _examNumbers)
                        {
                            var perfDate = exam.PerfDates.TryGetValue(examNum, out var pd) ? pd : null;
                            var examDate = exam.ExamDates.TryGetValue(examNum, out var ed) ? ed : null;

                            table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Background(bg).Padding(4).Text(FormatDate(perfDate)).Style(cellStyle);
                            table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Background(bg).Padding(4).Text(FormatDate(examDate)).Style(cellStyle);
                        }

                        rowIdx++;
                    }
                }
            });
        }

        void ComposeNote(IContainer container)
        {
            container
                .Background(Colors.Yellow.Lighten4)
                .BorderLeft(4)
                .BorderColor(Colors.Amber.Medium)
                .Padding(10)
                .Column(col =>
                {
                    col.Item().Text("Nota de comparación:").SemiBold().FontColor(Colors.Amber.Darken3);
                    col.Item().Text(
                        "Este reporte agrupa todos los exámenes del mismo CODI. " +
                        "Verifique concordancia entre fechas de perf y exam " +
                        "para detectar discrepancias en la toma de muestras y procesamiento."
                    ).FontSize(7).FontColor(Colors.Grey.Darken1);
                });
        }

        static string FormatDate(DateTime? date)
        {
            return date?.ToString("dd/MM/yyyy HH:mm") ?? "—";
        }

        void ComposeFooter(IContainer container)
        {
            container
                .BorderTop(0.5f)
                .BorderColor(Colors.Grey.Lighten2)
                .PaddingTop(8)
                .AlignCenter()
                .Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}  |  Sistema de Laboratorio  |  Confidencial")
                .FontSize(7).FontColor(Colors.Grey.Medium);
        }
    }
}

public class PatientReportDocument : IDocument
{
    private readonly PatientReport _report;

    public PatientReportDocument(PatientReport report)
    {
        _report = report;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container
            .Page(page =>
            {
                page.Margin(40);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Helvetica"));
                
                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
    }

    void ComposeHeader(IContainer container)
    {
        container
            .PaddingBottom(15)
            .BorderBottom(1.5f)
            .BorderColor(Colors.Blue.Darken3)
            .Column(col =>
            {
                col.Item().AlignCenter().Text("REPORTE DE EXÁMENES DE LABORATORIO")
                    .FontSize(16).Bold().FontColor(Colors.Blue.Darken3);
                
                col.Item().AlignCenter().Text($"Paciente (CODI): {_report.Codi}")
                    .FontSize(11).SemiBold();
                
                col.Item().AlignCenter().Text($"Período: {_report.FechaIni:dd/MM/yyyy} – {_report.FechaFin:dd/MM/yyyy}  |  Total exámenes: {_report.TotalExams}")
                    .FontSize(9).FontColor(Colors.Grey.Medium);
            });
    }

    void ComposeContent(IContainer container)
    {
        container.PaddingVertical(15).Column(col =>
        {
            col.Item().Element(ComposeExamsTable);
            col.Item().PaddingTop(15).Element(ComposeComparisonNote);
        });
    }

    void ComposeExamsTable(IContainer container)
    {
        var headerStyle = TextStyle.Default.SemiBold().FontSize(9);
        var cellStyle = TextStyle.Default.FontSize(9);
        
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(2f);
                columns.RelativeColumn(2f);
                columns.RelativeColumn(2f);
                columns.RelativeColumn(2f);
            });

            // Header row
            table.Header(header =>
            {
                header.Cell().Border(0.5f).BorderColor(Colors.Grey.Medium).Background(Colors.Blue.Lighten3).Padding(6).Text("Examen").Style(headerStyle);
                header.Cell().Border(0.5f).BorderColor(Colors.Grey.Medium).Background(Colors.Blue.Lighten3).Padding(6).Text("Perf 5\n(Fecha Auto)").Style(headerStyle);
                header.Cell().Border(0.5f).BorderColor(Colors.Grey.Medium).Background(Colors.Blue.Lighten3).Padding(6).Text("Perf 360\n(Fecha Auto)").Style(headerStyle);
                header.Cell().Border(0.5f).BorderColor(Colors.Grey.Medium).Background(Colors.Blue.Lighten3).Padding(6).Text("Exam 5\n(Fecha Auto)").Style(headerStyle);
                header.Cell().Border(0.5f).BorderColor(Colors.Grey.Medium).Background(Colors.Blue.Lighten3).Padding(6).Text("Exam 360\n(Fecha Auto)").Style(headerStyle);
            });

            // Data rows
            for (int i = 0; i < _report.Exams.Count; i++)
            {
                var exam = _report.Exams[i];
                var bgColor = i % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;
                
                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Background(bgColor).Padding(6).Text(exam.Exam.ToString()).Style(cellStyle).SemiBold();
                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Background(bgColor).Padding(6).Text(FormatDate(exam.PerfDates.TryGetValue(5, out var pd5) ? pd5 : null)).Style(cellStyle);
                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Background(bgColor).Padding(6).Text(FormatDate(exam.PerfDates.TryGetValue(360, out var pd360) ? pd360 : null)).Style(cellStyle);
                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Background(bgColor).Padding(6).Text(FormatDate(exam.ExamDates.TryGetValue(5, out var ed5) ? ed5 : null)).Style(cellStyle);
                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Background(bgColor).Padding(6).Text(FormatDate(exam.ExamDates.TryGetValue(360, out var ed360) ? ed360 : null)).Style(cellStyle);
            }
        });
    }

    void ComposeComparisonNote(IContainer container)
    {
        container
            .Background(Colors.Yellow.Lighten4)
            .BorderLeft(4)
            .BorderColor(Colors.Amber.Medium)
            .Padding(12)
            .Column(col =>
            {
                col.Item().Text("Nota de comparación:").SemiBold().FontColor(Colors.Amber.Darken3);
                col.Item().Text(
                    "Este reporte agrupa todos los exámenes del mismo CODI. " +
                    "Verifique concordancia entre fechas de perf (5/360) y exam (5/360) " +
                    "para detectar discrepancias en la toma de muestras y procesamiento."
                ).FontSize(8).FontColor(Colors.Grey.Darken1);
            });
    }

    void ComposeFooter(IContainer container)
    {
        container
            .BorderTop(0.5f)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingTop(10)
            .AlignCenter()
            .Text($"Generado: {_report.GeneratedAt:dd/MM/yyyy HH:mm}  |  Sistema de Laboratorio  |  Confidencial")
            .FontSize(7).FontColor(Colors.Grey.Medium);
    }

    static string FormatDate(DateTime? date)
    {
        return date?.ToString("dd/MM/yyyy HH:mm") ?? "—";
    }
}