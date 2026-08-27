using Hefesto.Core;

namespace Hefesto.Gui;

public class LoginForm : Form
{
    TextBox txtUser = new() { PlaceholderText = "Usuario" };
    TextBox txtPass = new() { PlaceholderText = "Contraseña", UseSystemPasswordChar = true };
    Label lblError = new() { ForeColor = Color.FromArgb(180, 40, 40), Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = false, TextAlign = ContentAlignment.MiddleCenter, Visible = false };
    public LoginForm()
    {
        Text = "Hefesto - Iniciar Sesión";
        ClientSize = new Size(420, 360);
        MinimumSize = new Size(420, 360);
        MaximumSize = new Size(420, 360);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; MinimizeBox = false;
        BackColor = Color.FromArgb(245, 245, 245);
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = new Font("Segoe UI", 9);

        var lblTitle = new Label { Text = "HEFESTO", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = Color.FromArgb(30, 60, 110), AutoSize = true, Location = new Point(145, 18) };
        var lblSub = new Label { Text = "Taller Mecánico", Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, AutoSize = true, Location = new Point(152, 52) };

        var lblUser = new Label { Text = "Usuario", Location = new Point(40, 85), AutoSize = true, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.FromArgb(60,60,60) };
        txtUser.Location = new Point(40, 105); txtUser.Size = new Size(340, 32); txtUser.Font = new Font("Segoe UI", 11);
        var lblPass = new Label { Text = "Contraseña", Location = new Point(40, 145), AutoSize = true, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.FromArgb(60,60,60) };
        txtPass.Location = new Point(40, 165); txtPass.Size = new Size(340, 32); txtPass.Font = new Font("Segoe UI", 11);

        lblError.Location = new Point(40, 205); lblError.Size = new Size(340, 20);

        var btn = new Button { Text = "ENTRAR", Location = new Point(40, 235), Size = new Size(340, 42), BackColor = Color.FromArgb(30, 60, 110), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
        btn.FlatAppearance.BorderSize = 0;

        var lblHint = new Label { Text = "Por defecto:  admin  /  admin123", Font = new Font("Segoe UI", 8), ForeColor = Color.FromArgb(120,120,120), AutoSize = false, TextAlign = ContentAlignment.MiddleCenter, Location = new Point(40, 285), Size = new Size(340, 18), BackColor = Color.FromArgb(235,235,235) };
        var lblDb = new Label { Text = $"Base: {Db.DbPath}", Font = new Font("Segoe UI", 6), ForeColor = Color.Gray, AutoSize = false, Location = new Point(10, 310), Size = new Size(400, 35) };
        lblDb.TextAlign = ContentAlignment.TopCenter;

        btn.Click += (s, e) => DoLogin();
        txtPass.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) DoLogin(); };
        txtUser.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) txtPass.Focus(); };
        AcceptButton = btn;
        txtUser.Text = "admin";
        txtPass.Text = "";

        Controls.AddRange(new Control[] { lblTitle, lblSub, lblUser, txtUser, lblPass, txtPass, lblError, btn, lblHint, lblDb });
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
                Close();
            }
            else
            {
                lblError.Text = "Usuario o contraseña incorrectos";
                lblError.Visible = true;
                // También mostrar MessageBox para que sea visible en cualquier DPI
                // MessageBox.Show("Usuario o contraseña incorrectos", "Acceso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPass.SelectAll();
                txtPass.Focus();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al validar login:\n{ex.Message}\n\nRuta DB: {Db.DbPath}\n\nSQLite ya está incluido (no requiere instalación). Verifique permisos de escritura.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
