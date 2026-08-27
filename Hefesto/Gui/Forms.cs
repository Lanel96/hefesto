using Hefesto.Core;

namespace Hefesto.Gui;

public class VehiculoForm : Form
{
    TextBox txtPlaca = new() { CharacterCasing = CharacterCasing.Upper };
    TextBox txtMarca = new();
    TextBox txtModelo = new();
    NumericUpDown txtAnio = new() { Minimum = 1900, Maximum = 2030, Value = 2020 };
    TextBox txtCliente = new();
    TextBox txtTel = new();
    public VehiculoForm(Vehiculo? v)
    {
        Text = v == null ? "Nuevo Vehículo" : $"Editar {v.Placa}";
        Size = new Size(420, 380); StartPosition = FormStartPosition.CenterParent; FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;
        int y = 15;
        void Add(string label, Control c) { Controls.Add(new Label { Text = label, Location = new Point(15, y), AutoSize = true, Font = new Font("Segoe UI", 8, FontStyle.Bold) }); c.Location = new Point(15, y + 18); c.Size = new Size(370, 28); c.Font = new Font("Segoe UI", 10); Controls.Add(c); y += 55; }
        Add("Placa *", txtPlaca); Add("Marca *", txtMarca); Add("Modelo *", txtModelo); Add("Año", txtAnio); Add("Cliente *", txtCliente); Add("Teléfono", txtTel);
        if (v != null) { txtPlaca.Text = v.Placa; txtPlaca.ReadOnly = true; txtPlaca.BackColor = Color.LightGray; txtMarca.Text = v.Marca; txtModelo.Text = v.Modelo; if (v.Anio.HasValue) txtAnio.Value = v.Anio.Value; txtCliente.Text = v.Cliente; txtTel.Text = v.Telefono; }
        var btn = new Button { Text = "💾 Guardar", Location = new Point(15, y + 5), Size = new Size(370, 36), BackColor = Color.FromArgb(0,150,80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
        btn.Click += (s, e) => { if (string.IsNullOrWhiteSpace(txtPlaca.Text) || string.IsNullOrWhiteSpace(txtMarca.Text) || string.IsNullOrWhiteSpace(txtModelo.Text) || string.IsNullOrWhiteSpace(txtCliente.Text)) { MessageBox.Show("Placa, Marca, Modelo y Cliente son obligatorios"); return; } Repos.UpsertVehiculo(new Vehiculo(txtPlaca.Text.Trim().ToUpper(), txtMarca.Text.Trim(), txtModelo.Text.Trim(), (int)txtAnio.Value, txtCliente.Text.Trim(), txtTel.Text.Trim())); DialogResult = DialogResult.OK; };
        Controls.Add(btn);
    }
}

public class ServicioForm : Form
{
    TextBox txtCodigo = new() { CharacterCasing = CharacterCasing.Upper };
    TextBox txtNombre = new();
    TextBox txtDesc = new();
    NumericUpDown txtPrecio = new() { DecimalPlaces = 2, Maximum = 100000, Minimum = 0 };
    NumericUpDown txtDur = new() { Maximum = 1000, Minimum = 5, Value = 60 };
    int id;
    public ServicioForm(Servicio? s)
    {
        Text = s == null ? "Nuevo Servicio" : $"Editar {s.Codigo}";
        Size = new Size(420, 360); StartPosition = FormStartPosition.CenterParent; FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;
        id = s?.Id ?? 0;
        int y = 15;
        void Add(string label, Control c) { Controls.Add(new Label { Text = label, Location = new Point(15, y), AutoSize = true, Font = new Font("Segoe UI", 8, FontStyle.Bold) }); c.Location = new Point(15, y + 18); c.Size = new Size(370, 28); c.Font = new Font("Segoe UI", 10); Controls.Add(c); y += 55; }
        Add("Código *", txtCodigo); Add("Nombre *", txtNombre); Add("Descripción", txtDesc); Add("Precio *", txtPrecio); Add("Duración (min)", txtDur);
        if (s != null) { txtCodigo.Text = s.Codigo; txtNombre.Text = s.Nombre; txtDesc.Text = s.Descripcion; txtPrecio.Value = s.Precio; txtDur.Value = s.DuracionMin; }
        var btn = new Button { Text = "💾 Guardar", Location = new Point(15, y + 5), Size = new Size(370, 36), BackColor = Color.FromArgb(0,150,80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
        btn.Click += (s, e) => { if (string.IsNullOrWhiteSpace(txtCodigo.Text) || string.IsNullOrWhiteSpace(txtNombre.Text)) { MessageBox.Show("Código y Nombre requeridos"); return; } try { Repos.SaveServicio(new Servicio(id, txtCodigo.Text.Trim().ToUpper(), txtNombre.Text.Trim(), txtDesc.Text.Trim(), txtPrecio.Value, (int)txtDur.Value)); DialogResult = DialogResult.OK; } catch (Exception ex) { MessageBox.Show(ex.Message); } };
        Controls.Add(btn);
    }
}

public class DetalleOrdenForm : Form
{
    public DetalleOrdenForm(int ordenId)
    {
        Text = $"Detalle Orden #{ordenId}"; Size = new Size(750, 550); StartPosition = FormStartPosition.CenterParent; BackColor = Color.White;
        var orden = Repos.GetOrdenes().First(o => o.Id == ordenId);
        var servs = Repos.GetOrdenServicios(ordenId);
        var reps = Repos.GetRepuestos(ordenId);
        var veh = Repos.GetVehiculos(orden.Placa).FirstOrDefault();

        var header = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.FromArgb(30,60,110), Padding = new Padding(15) };
        header.Controls.Add(new Label { Text = $"Orden #{orden.Id}  •  {orden.Placa}  •  {veh?.Marca} {veh?.Modelo}  •  {veh?.Cliente}", ForeColor = Color.White, Font = new Font("Segoe UI", 11, FontStyle.Bold), Dock = DockStyle.Top, Height = 25 });
        header.Controls.Add(new Label { Text = $"Ingreso: {orden.FechaIngreso:dd/MM/yyyy HH:mm}  •  Estado: {orden.Estado}  •  Total: {orden.Total:C}  •  Obs: {orden.Observaciones}", ForeColor = Color.FromArgb(200,220,255), Dock = DockStyle.Bottom, Height = 20 });

        var tabs = new TabControl { Dock = DockStyle.Fill };
        var tabServ = new TabPage("Servicios");
        var dgvS = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, DataSource = servs.Select(s => new { s.ServicioNombre, Precio = s.PrecioAplicado.ToString("C"), s.Cantidad, Subtotal = (s.PrecioAplicado * s.Cantidad).ToString("C") }).ToList() };
        tabServ.Controls.Add(dgvS);

        var tabRep = new TabPage("Repuestos / Garantías");
        var dgvR = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
        dgvR.DataSource = reps.Select(r => new { r.Codigo, r.Nombre, Garantía = r.DiasGarantia + " días", Inicio = r.FechaInicio.ToString("dd/MM/yyyy"), Fin = r.FechaFin.ToString("dd/MM/yyyy"), Estado = r.EstadoGarantia }).ToList();
        // colorear
        dgvR.DataBindingComplete += (s, e) => { foreach (DataGridViewRow row in dgvR.Rows) { var est = row.Cells["Estado"].Value?.ToString() ?? ""; row.DefaultCellStyle.BackColor = est.StartsWith("EN GARANTÍA") ? Color.FromArgb(220,255,220) : est.StartsWith("GARANTÍA VENCIDA") ? Color.FromArgb(255,220,220) : Color.White; } };
        tabRep.Controls.Add(dgvR);

        tabs.TabPages.Add(tabServ); tabs.TabPages.Add(tabRep);
        Controls.Add(tabs); Controls.Add(header);
    }
}

public class UsuarioForm : Form
{
    TextBox txtUser = new();
    TextBox txtPass = new() { UseSystemPasswordChar = true };
    ComboBox cmbRol = new() { DropDownStyle = ComboBoxStyle.DropDownList, Items = { "Admin", "Mecánico", "Cajero" } };
    public UsuarioForm()
    {
        Text = "Nuevo Usuario"; Size = new Size(360, 280); StartPosition = FormStartPosition.CenterParent; FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;
        cmbRol.SelectedIndex = 0;
        int y = 15;
        void Add(string l, Control c) { Controls.Add(new Label { Text = l, Location = new Point(15, y), AutoSize = true, Font = new Font("Segoe UI", 8, FontStyle.Bold) }); c.Location = new Point(15, y + 18); c.Size = new Size(310, 28); c.Font = new Font("Segoe UI", 10); Controls.Add(c); y += 55; }
        Add("Usuario *", txtUser); Add("Contraseña *", txtPass); Add("Rol", cmbRol);
        var lblInfo = new Label { Text = "Módulo personalizado: cada usuario tendrá su acceso", ForeColor = Color.Gray, Font = new Font("Segoe UI", 7, FontStyle.Italic), AutoSize = true, Location = new Point(15, y) }; Controls.Add(lblInfo); y += 20;
        var btn = new Button { Text = "💾 Guardar", Location = new Point(15, y + 5), Size = new Size(310, 34), BackColor = Color.FromArgb(0,150,80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
        btn.Click += (s, e) => { if (string.IsNullOrWhiteSpace(txtUser.Text) || string.IsNullOrWhiteSpace(txtPass.Text)) { MessageBox.Show("Usuario y contraseña requeridos"); return; } Repos.SaveUsuario(txtUser.Text.Trim(), txtPass.Text, cmbRol.SelectedItem!.ToString()!); DialogResult = DialogResult.OK; };
        Controls.Add(btn);
    }
    public UsuarioForm(string username, string rol) : this()
    {
        Text = $"Editar Usuario - {username}";
        txtUser.Text = username; txtUser.ReadOnly = true; txtUser.BackColor = Color.FromArgb(240,240,240);
        cmbRol.SelectedItem = rol;
        // en edición, contraseña es opcional: si se deja vacía, se mantiene
        var lbl = Controls.OfType<Label>().FirstOrDefault(l => l.Text == "Contraseña *");
        if (lbl != null) lbl.Text = "Nueva Contraseña (dejar vacío para mantener)";
    }
}
