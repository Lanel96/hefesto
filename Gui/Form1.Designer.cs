namespace OracleReportGenerator.Gui
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabConnection = new System.Windows.Forms.TabPage();
            this.grpOracle = new System.Windows.Forms.GroupBox();
            this.btnTestConnection = new System.Windows.Forms.Button();
            this.txtConnectionTimeout = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtDataSource = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtUser = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tabReport = new System.Windows.Forms.TabPage();
            this.grpReport = new System.Windows.Forms.GroupBox();
            this.btnOpenOutputFolder = new System.Windows.Forms.Button();
            this.txtOutputFolder = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.dtpFechaFin = new System.Windows.Forms.DateTimePicker();
            this.label7 = new System.Windows.Forms.Label();
            this.dtpFechaIni = new System.Windows.Forms.DateTimePicker();
            this.label6 = new System.Windows.Forms.Label();
            this.grpExams = new System.Windows.Forms.GroupBox();
            this.txtNewExamNumber = new System.Windows.Forms.TextBox();
            this.btnAddExam = new System.Windows.Forms.Button();
            this.btnRemoveExam = new System.Windows.Forms.Button();
            this.lstExamNumbers = new System.Windows.Forms.ListBox();
            this.label9 = new System.Windows.Forms.Label();
            this.tabSql = new System.Windows.Forms.TabPage();
            this.txtSqlFile = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.btnSaveConfig = new System.Windows.Forms.Button();
            this.tabLog = new System.Windows.Forms.TabPage();
            this.txtLog = new System.Windows.Forms.RichTextBox();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabControl.SuspendLayout();
            this.tabConnection.SuspendLayout();
            this.grpOracle.SuspendLayout();
            this.tabReport.SuspendLayout();
            this.grpReport.SuspendLayout();
            this.grpExams.SuspendLayout();
            this.tabSql.SuspendLayout();
            this.tabLog.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabConnection);
            this.tabControl.Controls.Add(this.tabReport);
            this.tabControl.Controls.Add(this.tabSql);
            this.tabControl.Controls.Add(this.tabLog);
            this.tabControl.Location = new System.Drawing.Point(12, 12);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(760, 450);
            this.tabControl.TabIndex = 0;
            // 
            // tabConnection
            // 
            this.tabConnection.Controls.Add(this.grpOracle);
            this.tabConnection.Location = new System.Drawing.Point(4, 24);
            this.tabConnection.Name = "tabConnection";
            this.tabConnection.Padding = new System.Windows.Forms.Padding(3);
            this.tabConnection.Size = new System.Drawing.Size(752, 422);
            this.tabConnection.TabIndex = 0;
            this.tabConnection.Text = "Conexión Oracle";
            this.tabConnection.UseVisualStyleBackColor = true;
            // 
            // grpOracle
            // 
            this.grpOracle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpOracle.Controls.Add(this.btnTestConnection);
            this.grpOracle.Controls.Add(this.txtConnectionTimeout);
            this.grpOracle.Controls.Add(this.label5);
            this.grpOracle.Controls.Add(this.txtDataSource);
            this.grpOracle.Controls.Add(this.label4);
            this.grpOracle.Controls.Add(this.txtPassword);
            this.grpOracle.Controls.Add(this.label3);
            this.grpOracle.Controls.Add(this.txtUser);
            this.grpOracle.Controls.Add(this.label2);
            this.grpOracle.Location = new System.Drawing.Point(6, 6);
            this.grpOracle.Name = "grpOracle";
            this.grpOracle.Size = new System.Drawing.Size(740, 180);
            this.grpOracle.TabIndex = 0;
            this.grpOracle.TabStop = false;
            this.grpOracle.Text = "Configuración de Conexión";
            // 
            // btnTestConnection
            // 
            this.btnTestConnection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTestConnection.BackColor = System.Drawing.Color.FromArgb(0, 122, 204);
            this.btnTestConnection.FlatAppearance.BorderSize = 0;
            this.btnTestConnection.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTestConnection.ForeColor = System.Drawing.Color.White;
            this.btnTestConnection.Location = new System.Drawing.Point(580, 30);
            this.btnTestConnection.Name = "btnTestConnection";
            this.btnTestConnection.Size = new System.Drawing.Size(140, 35);
            this.btnTestConnection.TabIndex = 8;
            this.btnTestConnection.Text = "Probar Conexión";
            this.btnTestConnection.UseVisualStyleBackColor = false;
            // 
            // txtConnectionTimeout
            // 
            this.txtConnectionTimeout.Location = new System.Drawing.Point(150, 130);
            this.txtConnectionTimeout.Name = "txtConnectionTimeout";
            this.txtConnectionTimeout.Size = new System.Drawing.Size(100, 23);
            this.txtConnectionTimeout.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(20, 133);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(104, 15);
            this.label5.TabIndex = 6;
            this.label5.Text = "Timeout (segundos):";
            // 
            // txtDataSource
            // 
            this.txtDataSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDataSource.Location = new System.Drawing.Point(150, 95);
            this.txtDataSource.Name = "txtDataSource";
            this.txtDataSource.Size = new System.Drawing.Size(570, 23);
            this.txtDataSource.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(20, 98);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 15);
            this.label4.TabIndex = 4;
            this.label4.Text = "Data Source:";
            // 
            // txtPassword
            // 
            this.txtPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPassword.Location = new System.Drawing.Point(150, 62);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '•';
            this.txtPassword.Size = new System.Drawing.Size(570, 23);
            this.txtPassword.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 65);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(70, 15);
            this.label3.TabIndex = 2;
            this.label3.Text = "Contraseña:";
            // 
            // txtUser
            // 
            this.txtUser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtUser.Location = new System.Drawing.Point(150, 30);
            this.txtUser.Name = "txtUser";
            this.txtUser.Size = new System.Drawing.Size(400, 23);
            this.txtUser.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 15);
            this.label2.TabIndex = 0;
            this.label2.Text = "Usuario:";
            // 
            // tabReport
            // 
            this.tabReport.Controls.Add(this.grpReport);
            this.tabReport.Controls.Add(this.grpExams);
            this.tabReport.Location = new System.Drawing.Point(4, 24);
            this.tabReport.Name = "tabReport";
            this.tabReport.Padding = new System.Windows.Forms.Padding(3);
            this.tabReport.Size = new System.Drawing.Size(752, 422);
            this.tabReport.TabIndex = 1;
            this.tabReport.Text = "Configuración Reporte";
            this.tabReport.UseVisualStyleBackColor = true;
            // 
            // grpReport
            // 
            this.grpReport.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpReport.Controls.Add(this.btnOpenOutputFolder);
            this.grpReport.Controls.Add(this.txtOutputFolder);
            this.grpReport.Controls.Add(this.label8);
            this.grpReport.Controls.Add(this.dtpFechaFin);
            this.grpReport.Controls.Add(this.label7);
            this.grpReport.Controls.Add(this.dtpFechaIni);
            this.grpReport.Controls.Add(this.label6);
            this.grpReport.Location = new System.Drawing.Point(6, 6);
            this.grpReport.Name = "grpReport";
            this.grpReport.Size = new System.Drawing.Size(740, 150);
            this.grpReport.TabIndex = 1;
            this.grpReport.TabStop = false;
            this.grpReport.Text = "Parámetros del Reporte";
            // 
            // btnOpenOutputFolder
            // 
            this.btnOpenOutputFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenOutputFolder.Location = new System.Drawing.Point(655, 95);
            this.btnOpenOutputFolder.Name = "btnOpenOutputFolder";
            this.btnOpenOutputFolder.Size = new System.Drawing.Size(75, 23);
            this.btnOpenOutputFolder.TabIndex = 6;
            this.btnOpenOutputFolder.Text = "Abrir";
            this.btnOpenOutputFolder.UseVisualStyleBackColor = true;
            // 
            // txtOutputFolder
            // 
            this.txtOutputFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOutputFolder.Location = new System.Drawing.Point(150, 95);
            this.txtOutputFolder.Name = "txtOutputFolder";
            this.txtOutputFolder.Size = new System.Drawing.Size(495, 23);
            this.txtOutputFolder.TabIndex = 5;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(20, 98);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(104, 15);
            this.label8.TabIndex = 4;
            this.label8.Text = "Carpeta de salida:";
            // 
            // dtpFechaFin
            // 
            this.dtpFechaFin.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpFechaFin.Location = new System.Drawing.Point(150, 60);
            this.dtpFechaFin.Name = "dtpFechaFin";
            this.dtpFechaFin.Size = new System.Drawing.Size(120, 23);
            this.dtpFechaFin.TabIndex = 3;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(20, 65);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(57, 15);
            this.label7.TabIndex = 2;
            this.label7.Text = "Fecha Fin:";
            // 
            // dtpFechaIni
            // 
            this.dtpFechaIni.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpFechaIni.Location = new System.Drawing.Point(150, 30);
            this.dtpFechaIni.Name = "dtpFechaIni";
            this.dtpFechaIni.Size = new System.Drawing.Size(120, 23);
            this.dtpFechaIni.TabIndex = 1;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(20, 33);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(68, 15);
            this.label6.TabIndex = 0;
            this.label6.Text = "Fecha Inicio:";
            // 
            // grpExams
            // 
            this.grpExams.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpExams.Controls.Add(this.txtNewExamNumber);
            this.grpExams.Controls.Add(this.btnAddExam);
            this.grpExams.Controls.Add(this.btnRemoveExam);
            this.grpExams.Controls.Add(this.lstExamNumbers);
            this.grpExams.Controls.Add(this.label9);
            this.grpExams.Location = new System.Drawing.Point(6, 162);
            this.grpExams.Name = "grpExams";
            this.grpExams.Size = new System.Drawing.Size(740, 250);
            this.grpExams.TabIndex = 2;
            this.grpExams.TabStop = false;
            this.grpExams.Text = "Números de Examen (para comparar)";
            // 
            // txtNewExamNumber
            // 
            this.txtNewExamNumber.Location = new System.Drawing.Point(20, 50);
            this.txtNewExamNumber.Name = "txtNewExamNumber";
            this.txtNewExamNumber.PlaceholderText = "Ej: 5, 360, 100...";
            this.txtNewExamNumber.Size = new System.Drawing.Size(120, 23);
            this.txtNewExamNumber.TabIndex = 4;
            this.txtNewExamNumber.TextChanged += new System.EventHandler(this.TxtNewExamNumber_TextChanged);
            // 
            // btnAddExam
            // 
            this.btnAddExam.Location = new System.Drawing.Point(150, 50);
            this.btnAddExam.Name = "btnAddExam";
            this.btnAddExam.Size = new System.Drawing.Size(75, 23);
            this.btnAddExam.TabIndex = 3;
            this.btnAddExam.Text = "Agregar";
            this.btnAddExam.UseVisualStyleBackColor = true;
            // 
            // btnRemoveExam
            // 
            this.btnRemoveExam.Location = new System.Drawing.Point(235, 50);
            this.btnRemoveExam.Name = "btnRemoveExam";
            this.btnRemoveExam.Size = new System.Drawing.Size(75, 23);
            this.btnRemoveExam.TabIndex = 2;
            this.btnRemoveExam.Text = "Quitar";
            this.btnRemoveExam.UseVisualStyleBackColor = true;
            // 
            // lstExamNumbers
            // 
            this.lstExamNumbers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstExamNumbers.FormattingEnabled = true;
            this.lstExamNumbers.ItemHeight = 15;
            this.lstExamNumbers.Location = new System.Drawing.Point(20, 85);
            this.lstExamNumbers.Name = "lstExamNumbers";
            this.lstExamNumbers.Size = new System.Drawing.Size(700, 139);
            this.lstExamNumbers.TabIndex = 1;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(20, 25);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(300, 15);
            this.label9.TabIndex = 0;
            this.label9.Text = "Lista de números de examen a comparar (mínimo 2):";
            // 
            // tabSql
            // 
            this.tabSql.Controls.Add(this.txtSqlFile);
            this.tabSql.Controls.Add(this.label10);
            this.tabSql.Controls.Add(this.btnSaveConfig);
            this.tabSql.Location = new System.Drawing.Point(4, 24);
            this.tabSql.Name = "tabSql";
            this.tabSql.Size = new System.Drawing.Size(752, 422);
            this.tabSql.TabIndex = 2;
            this.tabSql.Text = "Query SQL";
            this.tabSql.UseVisualStyleBackColor = true;
            // 
            // txtSqlFile
            // 
            this.txtSqlFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSqlFile.Location = new System.Drawing.Point(150, 20);
            this.txtSqlFile.Name = "txtSqlFile";
            this.txtSqlFile.Size = new System.Drawing.Size(570, 23);
            this.txtSqlFile.TabIndex = 2;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(20, 23);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(95, 15);
            this.label10.TabIndex = 1;
            this.label10.Text = "Archivo SQL:";
            // 
            // btnSaveConfig
            // 
            this.btnSaveConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveConfig.Location = new System.Drawing.Point(640, 380);
            this.btnSaveConfig.Name = "btnSaveConfig";
            this.btnSaveConfig.Size = new System.Drawing.Size(100, 35);
            this.btnSaveConfig.TabIndex = 0;
            this.btnSaveConfig.Text = "Guardar Config";
            this.btnSaveConfig.UseVisualStyleBackColor = true;
            // 
            // tabLog
            // 
            this.tabLog.Controls.Add(this.txtLog);
            this.tabLog.Location = new System.Drawing.Point(4, 24);
            this.tabLog.Name = "tabLog";
            this.tabLog.Size = new System.Drawing.Size(752, 422);
            this.tabLog.TabIndex = 3;
            this.tabLog.Text = "Log";
            this.tabLog.UseVisualStyleBackColor = true;
            // 
            // txtLog
            // 
            this.txtLog.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtLog.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            this.txtLog.Location = new System.Drawing.Point(0, 0);
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.Size = new System.Drawing.Size(752, 422);
            this.txtLog.TabIndex = 0;
            this.txtLog.Text = "";
            // 
            // btnGenerate
            // 
            this.btnGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGenerate.BackColor = System.Drawing.Color.FromArgb(0, 122, 204);
            this.btnGenerate.FlatAppearance.BorderSize = 0;
            this.btnGenerate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGenerate.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnGenerate.ForeColor = System.Drawing.Color.White;
            this.btnGenerate.Location = new System.Drawing.Point(620, 475);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(150, 40);
            this.btnGenerate.TabIndex = 1;
            this.btnGenerate.Text = "Generar Reportes";
            this.btnGenerate.UseVisualStyleBackColor = false;
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(12, 490);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(600, 15);
            this.progressBar.TabIndex = 2;
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
            this.statusStrip.Location = new System.Drawing.Point(0, 520);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(784, 22);
            this.statusStrip.TabIndex = 3;
            this.statusStrip.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(42, 17);
            this.lblStatus.Text = "Listo";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 542);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.tabControl);
            this.MinimumSize = new System.Drawing.Size(800, 580);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Generador de Reportes Oracle - Laboratorio";
            this.tabControl.ResumeLayout(false);
            this.tabConnection.ResumeLayout(false);
            this.grpOracle.ResumeLayout(false);
            this.grpOracle.PerformLayout();
            this.tabReport.ResumeLayout(false);
            this.grpReport.ResumeLayout(false);
            this.grpReport.PerformLayout();
            this.grpExams.ResumeLayout(false);
            this.grpExams.PerformLayout();
            this.tabSql.ResumeLayout(false);
            this.tabSql.PerformLayout();
            this.tabLog.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabConnection;
        private System.Windows.Forms.TabPage tabReport;
        private System.Windows.Forms.TabPage tabSql;
        private System.Windows.Forms.TabPage tabLog;
        private System.Windows.Forms.GroupBox grpOracle;
        private System.Windows.Forms.Button btnTestConnection;
        private System.Windows.Forms.TextBox txtConnectionTimeout;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtDataSource;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtUser;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox grpReport;
        private System.Windows.Forms.Button btnOpenOutputFolder;
        private System.Windows.Forms.TextBox txtOutputFolder;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.DateTimePicker dtpFechaFin;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.DateTimePicker dtpFechaIni;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox grpExams;
        private System.Windows.Forms.TextBox txtNewExamNumber;
        private System.Windows.Forms.Button btnAddExam;
        private System.Windows.Forms.Button btnRemoveExam;
        private System.Windows.Forms.ListBox lstExamNumbers;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtSqlFile;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button btnSaveConfig;
        private System.Windows.Forms.RichTextBox txtLog;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
    }
}