using Hefesto.Core;

namespace Hefesto.Gui;

public class LoginForm : Form
{
    TextBox txtUser = new() { PlaceholderText = "Usuario", BorderStyle = BorderStyle.FixedSingle };
    TextBox txtPass = new() { PlaceholderText = "Contraseña", UseSystemPasswordChar = true, BorderStyle = BorderStyle.FixedSingle };
    Label lblError = new() { ForeColor = Color.FromArgb(180, 40, 40), Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = false, TextAlign = ContentAlignment.MiddleCenter, Visible = false, Height = 22 };
    public LoginForm()
    {
        Text = "Hefesto - Iniciar Sesión";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; MinimizeBox = false;
        BackColor = Color.White;
        AutoScaleMode = AutoScaleMode.None;
        Font = new Font("Segoe UI", 9);
        DoubleBuffered = true;

        float dpiScale = 1f;
        using (var g = CreateGraphics()) dpiScale = g.DpiX / 96f;

        int w = (int)Math.Round(440 * dpiScale);
        int h = (int)Math.Round(440 * dpiScale);
        ClientSize = new Size(w, h);
        MinimumSize = new Size(w, h);

        int padX = (int)Math.Round(40 * dpiScale);
        int padY = (int)Math.Round(24 * dpiScale);

        var main = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6, Padding = new Padding(padX, padY, padX, padY), BackColor = Color.White };
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, (int)Math.Round(70 * dpiScale)));
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, (int)Math.Round(55 * dpiScale)));
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, (int)Math.Round(55 * dpiScale)));
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, (int)Math.Round(30 * dpiScale)));
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, (int)Math.Round(55 * dpiScale)));
        main.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // Logo
        var logoPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        var lblTitle = new Label { Text = "HEFESTO", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = Color.FromArgb(30, 60, 110), AutoSize = true, TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Bottom, Margin = new Padding(0, 0, 0, 2) };
        var lblSub = new Label { Text = "Taller Mecánico", Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, AutoSize = true, TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Top, Margin = new Padding(0, 2, 0, 0) };
        logoPanel.Controls.Add(lblTitle); logoPanel.Controls.Add(lblSub);
        main.Controls.Add(logoPanel, 0, 0);

        // Usuario
        var pUser = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Color.Transparent, Padding = new Padding(0) };
        pUser.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
        pUser.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        pUser.Controls.Add(new Label { Text = "Usuario", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60), Dock = DockStyle.Bottom, AutoSize = true }, 0, 0);
        txtUser.Dock = DockStyle.Fill; txtUser.Font = new Font("Segoe UI", 11); txtUser.Margin = new Padding(0, 2, 0, 0);
        pUser.Controls.Add(txtUser, 0, 1);
        main.Controls.Add(pUser, 0, 1);

        // Contraseña
        var pPass = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Color.Transparent, Padding = new Padding(0) };
        pPass.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
        pPass.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        pPass.Controls.Add(new Label { Text = "Contraseña", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60), Dock = DockStyle.Bottom, AutoSize = true }, 0, 0);
        txtPass.Dock = DockStyle.Fill; txtPass.Font = new Font("Segoe UI", 11); txtPass.Margin = new Padding(0, 2, 0, 0);
        pPass.Controls.Add(txtPass, 0, 1);
        main.Controls.Add(pPass, 0, 2);

        // Error
        lblError.Dock = DockStyle.Fill;
        main.Controls.Add(lblError, 0, 3);

        // Botón
        var btn = new Button { Text = "ENTRAR", Dock = DockStyle.Fill, BackColor = Color.FromArgb(30, 60, 110), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand, Height = 44 };
        btn.FlatAppearance.BorderSize = 0;
        btn.Click += (s, e) => DoLogin();
        main.Controls.Add(btn, 0, 4);

        // Hint + DB info
        var bottom = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Color.Transparent, Padding = new Padding(0, 8, 0, 0) };
        bottom.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        bottom.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var lblHint = new Label { Text = "Por defecto:  admin  /  admin123", Font = new Font("Segoe UI", 7.5F), ForeColor = Color.FromArgb(120, 120, 120), AutoSize = false, TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill, BackColor = Color.FromArgb(245, 245, 245) };
        var lblDb = new Label { Text = $"Base: {Db.DbPath}", Font = new Font("Segoe UI", 6), ForeColor = Color.Gray, AutoSize = false, TextAlign = ContentAlignment.TopCenter, Dock = DockStyle.Fill };
        bottom.Controls.Add(lblHint, 0, 0);
        bottom.Controls.Add(lblDb, 0, 1);
        main.Controls.Add(bottom, 0, 5);

        Controls.Add(main);

        AcceptButton = btn;
        txtPass.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) DoLogin(); };
        txtUser.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) txtPass.Focus(); };
        txtUser.Text = "admin";
        txtPass.Text = "";
        Shown += (s, e) => txtPass.Focus();
    }
    void DoLogin()
    {
        lblError.Visible = false;
        var user = txtUser.Text.Trim();
        var pass = txtPass.Text;
        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
        {
            lblError.Text = "Ingrese usuario y contraseña";
            lblError.Visible = true;
            return;
        }
        try
        {
            if (Repos.Validate(user, pass))
            {
                DialogResult = DialogResult.OK;
            }
            else
            {
                lblError.Text = "Usuario o contraseña incorrectos";
                lblError.Visible = true;
                txtPass.SelectAll();
                txtPass.Focus();
            }
        }
        catch (Exception ex)
        {
            lblError.Text = $"Error: {ex.Message}";
            lblError.Visible = true;
            MessageBox.Show($"Error al validar login:\n{ex.Message}\n\nRuta DB: {Db.DbPath}\n\nSQLite ya está incluido (no requiere instalación). Verifique permisos de escritura.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}