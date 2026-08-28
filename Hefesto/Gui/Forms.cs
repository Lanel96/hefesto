using Hefesto.Core;

namespace Hefesto.Gui;

public class VehiculoForm : Form
{
    TextBox txtPlaca = new() { CharacterCasing = CharacterCasing.Upper, BorderStyle = BorderStyle.FixedSingle };
    TextBox txtMarca = new() { BorderStyle = BorderStyle.FixedSingle };
    TextBox txtModelo = new() { BorderStyle = BorderStyle.FixedSingle };
    NumericUpDown txtAnio = new() { Minimum = 1900, Maximum = 2030, Value = 2020, BorderStyle = BorderStyle.FixedSingle };
    TextBox txtCliente = new() { BorderStyle = BorderStyle.FixedSingle };
    TextBox txtTel = new() { BorderStyle = BorderStyle.FixedSingle };
    public VehiculoForm(Vehiculo? v)
    {
        Text = v == null ? "Nuevo Vehículo" : $"Editar {v.Placa}";
        ClientSize = new Size(420, 460); MinimumSize = new Size(440, 500); StartPosition = FormStartPosition.CenterParent; FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false; AutoScaleMode = AutoScaleMode.Dpi; AutoScroll = true; BackColor = Color.White;
        int y = 15;
        void Add(string label, Control c) { Controls.Add(new Label { Text = label, Location = new Point(15, y), AutoSize = true, Font = new Font("Segoe UI", 8, FontStyle.Bold) }); c.Location = new Point(15, y + 18); c.Size = new Size(390, 28); c.Font = new Font("Segoe UI", 10); Controls.Add(c); y += 58; }
        Add("Placa *", txtPlaca); Add("Marca *", txtMarca); Add("Modelo *", txtModelo); Add("Año", txtAnio); Add("Cliente *", txtCliente); Add("Teléfono", txtTel);
        if (v != null) { txtPlaca.Text = v.Placa; txtPlaca.ReadOnly = true; txtPlaca.BackColor = Color.FromArgb(240,240,240); txtMarca.Text = v.Marca; txtModelo.Text = v.Modelo; if (v.Anio.HasValue) txtAnio.Value = v.Anio.Value; txtCliente.Text = v.Cliente; txtTel.Text = v.Telefono; }
        var btn = new Button { Text = "💾 Guardar", Location = new Point(15, y + 10), Size = new Size(390, 38), BackColor = Color.FromArgb(0,150,80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
        btn.FlatAppearance.BorderSize = 0;
        btn.Click += (s, e) => { if (string.IsNullOrWhiteSpace(txtPlaca.Text) || string.IsNullOrWhiteSpace(txtMarca.Text) || string.IsNullOrWhiteSpace(txtModelo.Text) || string.IsNullOrWhiteSpace(txtCliente.Text)) { MessageBox.Show("Placa, Marca, Modelo y Cliente son obligatorios"); return; } Repos.UpsertVehiculo(new Vehiculo(txtPlaca.Text.Trim().ToUpper(), txtMarca.Text.Trim(), txtModelo.Text.Trim(), (int)txtAnio.Value, txtCliente.Text.Trim(), txtTel.Text.Trim())); DialogResult = DialogResult.OK; };
        Controls.Add(btn);
    }
}

public class ServicioForm : Form
{
    TextBox txtCodigo = new() { CharacterCasing = CharacterCasing.Upper, BorderStyle = BorderStyle.FixedSingle };
    TextBox txtNombre = new() { BorderStyle = BorderStyle.FixedSingle };
    TextBox txtDesc = new() { BorderStyle = BorderStyle.FixedSingle };
    NumericUpDown txtPrecio = new() { DecimalPlaces = 2, Maximum = 100000, Minimum = 0, BorderStyle = BorderStyle.FixedSingle };
    NumericUpDown txtDur = new() { Maximum = 1000, Minimum = 5, Value = 60, BorderStyle = BorderStyle.FixedSingle };
    int id;
    public ServicioForm(Servicio? s)
    {
        Text = s == null ? "Nuevo Servicio" : $"Editar {s.Codigo}";
        ClientSize = new Size(420, 420); MinimumSize = new Size(440, 460); StartPosition = FormStartPosition.CenterParent; FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false; AutoScaleMode = AutoScaleMode.Dpi; AutoScroll = true; BackColor = Color.White;
        id = s?.Id ?? 0;
        int y = 15;
        void Add(string label, Control c) { Controls.Add(new Label { Text = label, Location = new Point(15, y), AutoSize = true, Font = new Font("Segoe UI", 8, FontStyle.Bold) }); c.Location = new Point(15, y + 18); c.Size = new Size(390, 28); c.Font = new Font("Segoe UI", 10); Controls.Add(c); y += 58; }
        Add("Código *", txtCodigo); Add("Nombre *", txtNombre); Add("Descripción", txtDesc); Add("Precio *", txtPrecio); Add("Duración (min)", txtDur);
        if (s != null) { txtCodigo.Text = s.Codigo; txtNombre.Text = s.Nombre; txtDesc.Text = s.Descripcion; txtPrecio.Value = s.Precio; txtDur.Value = s.DuracionMin; }
        var btn = new Button { Text = "💾 Guardar", Location = new Point(15, y + 10), Size = new Size(390, 38), BackColor = Color.FromArgb(0,150,80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
        btn.FlatAppearance.BorderSize = 0;
        btn.Click += (s, e) => { if (string.IsNullOrWhiteSpace(txtCodigo.Text) || string.IsNullOrWhiteSpace(txtNombre.Text)) { MessageBox.Show("Código y Nombre requeridos"); return; } try { Repos.SaveServicio(new Servicio(id, txtCodigo.Text.Trim().ToUpper(), txtNombre.Text.Trim(), txtDesc.Text.Trim(), txtPrecio.Value, (int)txtDur.Value)); DialogResult = DialogResult.OK; } catch (Exception ex) { MessageBox.Show(ex.Message); } };
        Controls.Add(btn);
    }
}

public class InventarioForm : Form
{
    TextBox txtCodigo = new() { CharacterCasing = CharacterCasing.Upper, BorderStyle = BorderStyle.FixedSingle };
    TextBox txtNombre = new() { BorderStyle = BorderStyle.FixedSingle };
    NumericUpDown txtExist = new() { Minimum = 0, Maximum = 100000, Value = 0, BorderStyle = BorderStyle.FixedSingle };
    NumericUpDown txtPrecio = new() { DecimalPlaces = 2, Maximum = 1000000, Minimum = 0, BorderStyle = BorderStyle.FixedSingle };
    int id;
    public InventarioForm(InventarioItem? it)
    {
        Text = it == null ? "Nuevo Artículo Inventario" : $"Editar {it.Codigo}";
        ClientSize = new Size(420, 320); MinimumSize = new Size(440, 360); StartPosition = FormStartPosition.CenterParent; FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false; AutoScaleMode = AutoScaleMode.Dpi; AutoScroll = true; BackColor = Color.White;
        id = it?.Id ?? 0;
        int y = 15;
        void Add(string label, Control c) { Controls.Add(new Label { Text = label, Location = new Point(15, y), AutoSize = true, Font = new Font("Segoe UI", 8, FontStyle.Bold) }); c.Location = new Point(15, y + 18); c.Size = new Size(390, 28); c.Font = new Font("Segoe UI", 10); Controls.Add(c); y += 58; }
        Add("Código *", txtCodigo); Add("Nombre *", txtNombre); Add("Existencia *", txtExist); Add("Precio *", txtPrecio);
        if (it != null) { txtCodigo.Text = it.Codigo; txtNombre.Text = it.Nombre; txtExist.Value = it.Existencia; txtPrecio.Value = it.Precio; }
        var btn = new Button { Text = "💾 Guardar", Location = new Point(15, y + 10), Size = new Size(390, 38), BackColor = Color.FromArgb(0,150,80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
        btn.FlatAppearance.BorderSize = 0;
        btn.Click += (s, e) => { if (string.IsNullOrWhiteSpace(txtCodigo.Text) || string.IsNullOrWhiteSpace(txtNombre.Text)) { MessageBox.Show("Código y Nombre requeridos"); return; } try { Repos.SaveInventario(new InventarioItem(id, txtCodigo.Text.Trim().ToUpper(), txtNombre.Text.Trim(), (int)txtExist.Value, txtPrecio.Value)); DialogResult = DialogResult.OK; } catch (Exception ex) { MessageBox.Show(ex.Message); } };
        Controls.Add(btn);
    }
}

public class BuscarVehiculoForm : Form
{
    public Vehiculo? Seleccionado { get; private set; }
    TextBox txtPlaca = new() { CharacterCasing = CharacterCasing.Upper, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "Ingrese placa..." };
    DataGridView dgv = new() { Dock = DockStyle.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle, RowHeadersVisible = false };
    public BuscarVehiculoForm(string? placaInicial = null)
    {
        Text = "🔍 Buscar Vehículo por Placa"; ClientSize = new Size(620, 400); MinimumSize = new Size(620, 400); StartPosition = FormStartPosition.CenterParent; FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false; AutoScaleMode = AutoScaleMode.Dpi; BackColor = Color.White;
        txtPlaca.Text = placaInicial ?? "";
        var top = new Panel { Height = 45, Dock = DockStyle.Top, Padding = new Padding(10) };
        var btnBuscar = new Button { Text = "🔍 Buscar", Size = new Size(90, 28), BackColor = Color.FromArgb(30,60,110), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Location = new Point(320, 8) };
        var btnNuevo = new Button { Text = "➕ Nuevo", Size = new Size(90, 28), BackColor = Color.FromArgb(0,150,80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Location = new Point(415, 8) };
        txtPlaca.Location = new Point(10, 8); txtPlaca.Size = new Size(300, 28);
        var lbl = new Label { Text = "Placa:", Location = new Point(10, 12), AutoSize = true, Font = new Font("Segoe UI", 8, FontStyle.Bold) };
        // actually lbl not needed
        top.Controls.AddRange(new Control[] { txtPlaca, btnBuscar, btnNuevo });
        var bottom = new Panel { Height = 45, Dock = DockStyle.Bottom, Padding = new Padding(10) };
        var btnSel = new Button { Text = "Seleccionar", Dock = DockStyle.Right, Width = 120, BackColor = Color.FromArgb(30,60,110), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
        var btnCancel = new Button { Text = "Cancelar", Dock = DockStyle.Right, Width = 90, FlatStyle = FlatStyle.Flat, Margin = new Padding(0,0,8,0) };
        bottom.Controls.Add(btnSel); bottom.Controls.Add(btnCancel);
        Controls.Add(dgv); Controls.Add(top); Controls.Add(bottom);
        dgv.DoubleClick += (s,e) => Seleccionar();
        btnBuscar.Click += (s,e) => Buscar();
        btnNuevo.Click += (s,e) => { using var f=new VehiculoForm(null); if(f.ShowDialog()==DialogResult.OK) Buscar(); };
        btnSel.Click += (s,e) => Seleccionar();
        btnCancel.Click += (s,e) => DialogResult = DialogResult.Cancel;
        txtPlaca.KeyDown += (s,e) => { if(e.KeyCode==Keys.Enter) Buscar(); };
        if(!string.IsNullOrWhiteSpace(placaInicial)) Buscar();
    }
    void Buscar()
    {
        var filtro = txtPlaca.Text.Trim();
        var list = string.IsNullOrWhiteSpace(filtro) ? Repos.GetVehiculos() : Repos.GetVehiculos(filtro);
        dgv.DataSource = list.Select(v => new { v.Placa, v.Marca, v.Modelo, v.Cliente, v.Telefono }).ToList();
        if(list.Count==0 && !string.IsNullOrWhiteSpace(filtro))
        {
            if(MessageBox.Show($"No se encontró placa '{filtro}'. ¿Desea crear un nuevo vehículo?", "No encontrado", MessageBoxButtons.YesNo, MessageBoxIcon.Question)==DialogResult.Yes)
            {
                using var f=new VehiculoForm(null);
                // prellenar placa
                // VehiculoForm no expone, pero podemos cerrar y dejar que usuario cree
                if(f.ShowDialog()==DialogResult.OK) Buscar();
            }
        }
    }
    void Seleccionar()
    {
        if(dgv.CurrentRow==null) return;
        var placa = dgv.CurrentRow.Cells["Placa"].Value?.ToString();
        if(placa==null) return;
        Seleccionado = Repos.GetVehiculos(placa).FirstOrDefault(x=>x.Placa==placa);
        DialogResult = DialogResult.OK;
    }
}

public class BuscarInventarioForm : Form
{
    public InventarioItem? Seleccionado { get; private set; }
    TextBox txtFiltro = new() { PlaceholderText = "Buscar por código o nombre...", BorderStyle = BorderStyle.FixedSingle };
    DataGridView dgv = new() { Dock = DockStyle.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle, RowHeadersVisible = false };
    public BuscarInventarioForm()
    {
        Text = "🔍 Buscar en Inventario"; ClientSize = new Size(650, 400); MinimumSize = new Size(650, 400); StartPosition = FormStartPosition.CenterParent; FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false; AutoScaleMode = AutoScaleMode.Dpi; BackColor = Color.White;
        var top = new Panel { Height = 45, Dock = DockStyle.Top, Padding = new Padding(10) };
        txtFiltro.Location = new Point(10, 8); txtFiltro.Size = new Size(350, 28);
        var btnBuscar = new Button { Text = "🔍 Buscar", Size = new Size(90, 28), BackColor = Color.FromArgb(30,60,110), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Location = new Point(370, 8) };
        top.Controls.AddRange(new Control[] { txtFiltro, btnBuscar });
        var bottom = new Panel { Height = 45, Dock = DockStyle.Bottom, Padding = new Padding(10) };
        var btnSel = new Button { Text = "Seleccionar", Dock = DockStyle.Right, Width = 120, BackColor = Color.FromArgb(30,60,110), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
        var btnCancel = new Button { Text = "Cancelar", Dock = DockStyle.Right, Width = 90, FlatStyle = FlatStyle.Flat };
        bottom.Controls.Add(btnSel); bottom.Controls.Add(btnCancel);
        Controls.Add(dgv); Controls.Add(top); Controls.Add(bottom);
        txtFiltro.TextChanged += (s,e) => Buscar();
        btnBuscar.Click += (s,e) => Buscar();
        dgv.DoubleClick += (s,e) => Seleccionar();
        btnSel.Click += (s,e) => Seleccionar();
        btnCancel.Click += (s,e) => DialogResult = DialogResult.Cancel;
        txtFiltro.KeyDown += (s,e) => { if(e.KeyCode==Keys.Enter) Seleccionar(); };
        Buscar();
    }
    void Buscar()
    {
        var list = Repos.GetInventario(txtFiltro.Text.Trim());
        dgv.DataSource = list.Select(x => new { x.Codigo, x.Nombre, x.Existencia, Precio = x.Precio.ToString("C") }).ToList();
    }
    void Seleccionar()
    {
        if(dgv.CurrentRow==null) return;
        var cod = dgv.CurrentRow.Cells["Codigo"].Value?.ToString();
        if(cod==null) return;
        Seleccionado = Repos.GetInventarioByCodigo(cod);
        DialogResult = DialogResult.OK;
    }
}

public class DetalleOrdenForm : Form
{
    public DetalleOrdenForm(int ordenId)
    {
        Text = $"Detalle Orden #{ordenId}"; ClientSize = new Size(750, 550); MinimumSize = new Size(760, 560); StartPosition = FormStartPosition.CenterParent; BackColor = Color.White; AutoScaleMode = AutoScaleMode.Dpi; AutoScroll = true;
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
    TextBox txtUser = new() { BorderStyle = BorderStyle.FixedSingle };
    TextBox txtPass = new() { UseSystemPasswordChar = true, BorderStyle = BorderStyle.FixedSingle };
    ComboBox cmbRol = new() { DropDownStyle = ComboBoxStyle.DropDownList, Items = { "Admin", "Mecánico", "Cajero" } };
    public UsuarioForm()
    {
        Text = "Nuevo Usuario"; ClientSize = new Size(360, 300); MinimumSize = new Size(380, 340); StartPosition = FormStartPosition.CenterParent; FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false; AutoScaleMode = AutoScaleMode.Dpi; AutoScroll = true; BackColor = Color.White;
        cmbRol.SelectedIndex = 0;
        int y = 15;
        void Add(string l, Control c) { Controls.Add(new Label { Text = l, Location = new Point(15, y), AutoSize = true, Font = new Font("Segoe UI", 8, FontStyle.Bold) }); c.Location = new Point(15, y + 18); c.Size = new Size(330, 28); c.Font = new Font("Segoe UI", 10); Controls.Add(c); y += 58; }
        Add("Usuario *", txtUser); Add("Contraseña *", txtPass); Add("Rol", cmbRol);
        var lblInfo = new Label { Text = "Módulo personalizado: cada usuario tendrá su acceso", ForeColor = Color.Gray, Font = new Font("Segoe UI", 7, FontStyle.Italic), AutoSize = true, Location = new Point(15, y) }; Controls.Add(lblInfo); y += 22;
        var btn = new Button { Text = "💾 Guardar", Location = new Point(15, y + 5), Size = new Size(330, 36), BackColor = Color.FromArgb(0,150,80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
        btn.FlatAppearance.BorderSize = 0;
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
