using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OracleReportGenerator.Core.Interfaces;
using OracleReportGenerator.Core.Models;
using OracleReportGenerator.Core.Services;
using QuestPDF.Fluent;
using OracleReportGenerator.Core.Reports;

namespace OracleReportGenerator.Gui
{
    public partial class MainForm : Form
    {
        private readonly IHost _host;
        private readonly ILogger<MainForm> _logger;
        private readonly IOracleRepository _repository;
        private readonly IReportValidator _validator;
        private readonly IReportGrouper _grouper;
        private readonly IReportGenerator _generator;
        private readonly IOptions<OracleConnectionConfig> OracleConfigOptions;
        private readonly IOptions<ReportConfig> ReportConfigOptions;
        
        private bool _isGenerating = false;

        private OracleConnectionConfig OracleConfig => OracleConfigOptions.Value;
        private ReportConfig ReportConfig => ReportConfigOptions.Value;

        public MainForm()
        {
            InitializeComponent();
            
            // Build host with DI
            var builder = Host.CreateApplicationBuilder();
            builder.Configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            
            builder.Services.AddOptions();
            builder.Services.Configure<OracleConnectionConfig>(builder.Configuration.GetSection("Oracle"));
            builder.Services.Configure<ReportConfig>(builder.Configuration.GetSection("Report"));
            
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.SetMinimumLevel(LogLevel.Information);
            
            builder.Services.AddSingleton<IOracleRepository, OracleRepository>();
            builder.Services.AddSingleton<IReportValidator, ReportValidator>();
            builder.Services.AddSingleton<IReportGrouper, ReportGrouper>();
            builder.Services.AddSingleton<IReportGenerator, QuestPdfReportGenerator>();
            
            _host = builder.Build();
            
            // Get services
            _logger = _host.Services.GetRequiredService<ILogger<MainForm>>();
            _repository = _host.Services.GetRequiredService<IOracleRepository>();
            _validator = _host.Services.GetRequiredService<IReportValidator>();
            _grouper = _host.Services.GetRequiredService<IReportGrouper>();
            _generator = _host.Services.GetRequiredService<IReportGenerator>();
            OracleConfigOptions = _host.Services.GetRequiredService<IOptions<OracleConnectionConfig>>();
            ReportConfigOptions = _host.Services.GetRequiredService<IOptions<ReportConfig>>();
            
            // Load config into UI
            LoadConfigToUi();
            
            // Setup event handlers
            btnTestConnection.Click += BtnTestConnection_Click;
            btnGenerate.Click += BtnGenerate_Click;
            btnOpenOutputFolder.Click += BtnOpenOutputFolder_Click;
            btnSaveConfig.Click += BtnSaveConfig_Click;
            btnAddExam.Click += BtnAddExam_Click;
            btnRemoveExam.Click += BtnRemoveExam_Click;
            lstExamNumbers.SelectedIndexChanged += LstExamNumbers_SelectedIndexChanged;
            
            // Set default output folder
            txtOutputFolder.Text = Path.GetFullPath(ReportConfig.OutputDirectory);
        }

        private void LoadConfigToUi()
        {
            txtUser.Text = OracleConfig.User;
            txtPassword.Text = OracleConfig.Password;
            txtDataSource.Text = OracleConfig.DataSource;
            txtConnectionTimeout.Text = OracleConfig.ConnectionTimeout.ToString();
            
            dtpFechaIni.Value = ReportConfig.FechaIni;
            dtpFechaFin.Value = ReportConfig.FechaFin;
            txtOutputFolder.Text = Path.GetFullPath(ReportConfig.OutputDirectory);
            txtSqlFile.Text = ReportConfig.SqlFilePath;
            
            lstExamNumbers.Items.Clear();
            foreach (var exam in ReportConfig.ExamNumbers)
            {
                lstExamNumbers.Items.Add(exam);
            }
            
            UpdateExamButtons();
        }

        private void SaveUiToConfig()
        {
            OracleConfig.User = txtUser.Text.Trim();
            OracleConfig.Password = txtPassword.Text;
            OracleConfig.DataSource = txtDataSource.Text.Trim();
            OracleConfig.ConnectionTimeout = int.TryParse(txtConnectionTimeout.Text, out var t) ? t : 30;
            
            ReportConfig.FechaIni = dtpFechaIni.Value.Date;
            ReportConfig.FechaFin = dtpFechaFin.Value.Date;
            ReportConfig.OutputDirectory = txtOutputFolder.Text.Trim();
            ReportConfig.SqlFilePath = txtSqlFile.Text.Trim();
            
            ReportConfig.ExamNumbers = lstExamNumbers.Items.Cast<int>().ToList();
        }

        private void UpdateExamButtons()
        {
            btnRemoveExam.Enabled = lstExamNumbers.SelectedIndex >= 0;
            btnAddExam.Enabled = int.TryParse(txtNewExamNumber.Text, out _) && 
                                  !lstExamNumbers.Items.Cast<int>().Contains(int.Parse(txtNewExamNumber.Text));
        }

        private async void BtnTestConnection_Click(object? sender, EventArgs e)
        {
            SaveUiToConfig();
            
            btnTestConnection.Enabled = false;
            btnTestConnection.Text = "Probando...";
            AppendLog("Probando conexión a Oracle...");
            
            try
            {
                var connected = await _repository.TestConnectionAsync();
                if (connected)
                {
                    AppendLog("✓ Conexión exitosa", Color.Green);
                    MessageBox.Show("Conexión a Oracle exitosa", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    AppendLog("✗ Conexión fallida", Color.Red);
                    MessageBox.Show("No se pudo conectar a Oracle. Verifique credenciales.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                AppendLog($"✗ Error: {ex.Message}", Color.Red);
                MessageBox.Show($"Error de conexión: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnTestConnection.Enabled = true;
                btnTestConnection.Text = "Probar Conexión";
            }
        }

        private async void BtnGenerate_Click(object? sender, EventArgs e)
        {
            if (_isGenerating) return;
            
            SaveUiToConfig();
            
            if (ReportConfig.ExamNumbers.Count < 2)
            {
                MessageBox.Show("Se requieren al menos 2 números de examen para la comparación.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            if (string.IsNullOrWhiteSpace(OracleConfig.User) || string.IsNullOrWhiteSpace(OracleConfig.DataSource))
            {
                MessageBox.Show("Configure la conexión a Oracle primero.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            _isGenerating = true;
            SetGeneratingState(true);
            progressBar.Value = 0;
            progressBar.Style = ProgressBarStyle.Marquee;
            AppendLog("=== Iniciando generación de reportes ===");
            AppendLog($"Período: {ReportConfig.FechaIni:dd/MM/yyyy} - {ReportConfig.FechaFin:dd/MM/yyyy}");
            AppendLog($"Exámenes: {string.Join(", ", ReportConfig.ExamNumbers)}");
            
            try
            {
                // Test connection
                AppendLog("Probando conexión...");
                var connected = await _repository.TestConnectionAsync();
                if (!connected)
                {
                    AppendLog("✗ No se pudo conectar a Oracle", Color.Red);
                    return;
                }
                AppendLog("✓ Conexión OK", Color.Green);
                
                // Get data
                AppendLog("Obteniendo datos de Oracle...");
                var records = await _repository.GetExamRecordsAsync(ReportConfig);
                AppendLog($"Registros obtenidos: {records.Count}");
                
                // Validate
                AppendLog("Validando datos...");
                var validation = _validator.Validate(records, ReportConfig);
                
                foreach (var w in validation.Warnings)
                    AppendLog($"⚠ {w}", Color.Orange);
                
                if (!validation.IsValid)
                {
                    foreach (var err in validation.Errors)
                        AppendLog($"✗ {err}", Color.Red);
                    return;
                }
                
                AppendLog($"✓ Válidos: {validation.ValidCount}/{validation.OriginalCount} | CODI únicos: {validation.UniqueCodis}", Color.Green);
                
                // Group by CODI
                AppendLog("Agrupando por CODI...");
                var reports = _grouper.GroupByCodi(
                    records.Where(r => !string.IsNullOrWhiteSpace(r.Codi)).ToList(),
                    ReportConfig.FechaIni, ReportConfig.FechaFin);
                
                AppendLog($"Reportes a generar: {reports.Count}");
                
                // Generate PDFs
                progressBar.Style = ProgressBarStyle.Continuous;
                progressBar.Maximum = 1;
                progressBar.Value = 0;
                
                AppendLog("Generando reporte global PDF único...");
                var outputDir = ReportConfig.OutputDirectory;
                Directory.CreateDirectory(outputDir);
                var globalOutputPath = Path.Combine(outputDir, "reporte_global.pdf");
                var uniqueExamNumbers = ReportConfig.ExamNumbers.Distinct().ToList();
                
                AppendLog($"Generando reporte para {reports.Count} pacientes...");
                
                try
                {
                    await _generator.GenerateGlobalReportAsync(reports, uniqueExamNumbers, globalOutputPath);
                    
                    if (_isGenerating)
                    {
                        AppendLog($"✓ Completado: Reporte global generado en {Path.GetFullPath(globalOutputPath)}", Color.Green);
                        MessageBox.Show($"Se generó el reporte global exitosamente.\n\nArchivo: {Path.GetFullPath(globalOutputPath)}", 
                            "Completado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generando reporte global");
                    AppendLog($"✗ Error fatal: {ex.Message}", Color.Red);
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando reportes");
                AppendLog($"✗ Error fatal: {ex.Message}", Color.Red);
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isGenerating = false;
                SetGeneratingState(false);
                progressBar.Style = ProgressBarStyle.Continuous;
                progressBar.Value = 0;
            }
        }

        private void BtnOpenOutputFolder_Click(object? sender, EventArgs e)
        {
            var path = Path.GetFullPath(txtOutputFolder.Text);
            if (Directory.Exists(path))
            {
                System.Diagnostics.Process.Start("explorer.exe", path);
            }
            else
            {
                MessageBox.Show("La carpeta no existe aún. Genere reportes primero.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnSaveConfig_Click(object? sender, EventArgs e)
        {
            try
            {
                SaveUiToConfig();
                
                // Update appsettings.json
                var configPath = "appsettings.json";
                var json = File.ReadAllText(configPath);
                
                // Simple JSON update (in production, use System.Text.Json)
                var lines = File.ReadAllLines(configPath).ToList();
                // For simplicity, just show success - full JSON editing would need more code
                AppendLog("Configuración guardada en memoria. Para persistir, edite appsettings.json manualmente.", Color.Blue);
                MessageBox.Show("Configuración actualizada en memoria.\nPara guardar permanentemente, edite el archivo appsettings.json", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error guardando: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAddExam_Click(object? sender, EventArgs e)
        {
            if (int.TryParse(txtNewExamNumber.Text, out var num) && num > 0)
            {
                if (!lstExamNumbers.Items.Cast<int>().Contains(num))
                {
                    lstExamNumbers.Items.Add(num);
                    txtNewExamNumber.Clear();
                    UpdateExamButtons();
                }
            }
        }

        private void BtnRemoveExam_Click(object? sender, EventArgs e)
        {
            if (lstExamNumbers.SelectedIndex >= 0)
            {
                lstExamNumbers.Items.RemoveAt(lstExamNumbers.SelectedIndex);
                UpdateExamButtons();
            }
        }

        private void LstExamNumbers_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateExamButtons();
        }

        private void TxtNewExamNumber_TextChanged(object? sender, EventArgs e)
        {
            UpdateExamButtons();
        }

        private void SetGeneratingState(bool generating)
        {
            btnGenerate.Enabled = !generating;
            btnGenerate.Text = generating ? "Generando..." : "Generar Reportes";
            btnTestConnection.Enabled = !generating;
            btnAddExam.Enabled = !generating;
            btnRemoveExam.Enabled = !generating && lstExamNumbers.SelectedIndex >= 0;
            txtNewExamNumber.Enabled = !generating;
            lstExamNumbers.Enabled = !generating;
            grpOracle.Enabled = !generating;
            grpReport.Enabled = !generating;
        }

        private void AppendLog(string message, Color? color = null)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(() => AppendLog(message, color));
                return;
            }
            
            var time = DateTime.Now.ToString("HH:mm:ss");
            var fullMsg = $"[{time}] {message}\n";
            
            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.SelectionLength = 0;
            txtLog.SelectionColor = color ?? txtLog.ForeColor;
            txtLog.AppendText(fullMsg);
            txtLog.ScrollToCaret();
        }

        private static string SanitizeFileName(string fileName)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                fileName = fileName.Replace(c, '_');
            return fileName;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_isGenerating)
            {
                var result = MessageBox.Show("¿Cancelar generación en curso y salir?", "Confirmar", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
                _isGenerating = false;
            }
            _host?.Dispose();
            base.OnFormClosing(e);
        }
    }
}