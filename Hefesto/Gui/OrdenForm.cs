using Hefesto.Core;

namespace Hefesto.Gui;

public class OrdenForm : Form
{
    ComboBox cmbPlaca = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    TextBox txtCliente = new() { ReadOnly = true, BackColor = Color.FromArgb(240,240,240) };
    DataGridView dgvServicios = new() { AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, Height = 150 };
    DataGridView dgvSeleccionados = new() { AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, Height = 140 };
    DataGridView dgvRepuestos = new() { AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, Height = 120 };
    Label lblTotal = new() { Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.FromArgb(0,120,60), AutoSize = true };
    TextBox txtObs = new() { PlaceholderText = "Observaciones de la orden..." };

    List<Servicio> catalogo = new();
    List<(Servicio s, int cant, decimal precio)> seleccion = new();
    List<(string codigo, string nombre, int dias, DateTime inicio)> repuestos = new();

    public OrdenForm()
    {
        Text = "➕ Nueva Orden - Seleccione vehículo y servicios";
        Size = new Size(960, 820);
        MinimumSize = new Size(920, 680);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9);
        AutoScroll = true;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = true;
        AutoScaleMode = AutoScaleMode.Dpi;

        var pTop = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.FromArgb(245,245,245), Padding = new Padding(10) };
        var lblPlaca = new Label { Text = "Vehículo (Placa):", Location = new Point(10, 10), AutoSize = true, Font = new Font("Segoe UI", 8, FontStyle.Bold) };
        cmbPlaca.Location = new Point(10, 30); cmbPlaca.Size = new Size(160, 28);
        var btnNuevoVeh = new Button { Text = "🚗 Nuevo Vehículo", Location = new Point(175, 30), Size = new Size(130, 28), BackColor = Color.FromArgb(30,60,110), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        btnNuevoVeh.Click += (s, e) => { using var f = new VehiculoForm(null); if (f.ShowDialog() == DialogResult.OK) CargarVehiculos(); };
        txtCliente.Location = new Point(315, 30); txtCliente.Size = new Size(300, 28); txtCliente.PlaceholderText = "Cliente / Marca Modelo";
        cmbPlaca.SelectedIndexChanged += (s, e) => ActualizarCliente();
        pTop.Controls.AddRange(new Control[] { lblPlaca, cmbPlaca, btnNuevoVeh, txtCliente });

        // Catálogo
        var lblCat = new Label { Text = "1️⃣ Catálogo de Servicios (doble clic para agregar - precio editable al agregar)", Dock = DockStyle.Top, Height = 22, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.FromArgb(30,60,110) };
        var pCat = new Panel { Dock = DockStyle.Top, Height = 170, Padding = new Padding(10,0,10,0) };
        var txtFiltro = new TextBox { PlaceholderText = "Filtrar servicios...", Dock = DockStyle.Top, Height = 28 };
        txtFiltro.TextChanged += (s, e) => FiltrarServicios(txtFiltro.Text);
        dgvServicios.Dock = DockStyle.Fill;
        dgvServicios.DoubleClick += (s, e) => AgregarServicio();
        pCat.Controls.Add(dgvServicios); pCat.Controls.Add(txtFiltro);

        // Seleccionados
        var pSel = new Panel { Dock = DockStyle.Top, Height = 180, Padding = new Padding(10,0,10,0) };
        var lblSel = new Label { Text = "2️⃣ Servicios Seleccionados (precio congelado para esta orden)", Dock = DockStyle.Top, Height = 22, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.FromArgb(0,120,60) };
        var btnQuitar = new Button { Text = "Quitar", Dock = DockStyle.Bottom, Height = 28, BackColor = Color.FromArgb(180,40,40), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        btnQuitar.Click += (s, e) => QuitarServicio();
        dgvSeleccionados.Dock = DockStyle.Fill;
        pSel.Controls.Add(dgvSeleccionados); pSel.Controls.Add(btnQuitar); pSel.Controls.Add(lblSel);

        // Repuestos garantía
        var pRep = new Panel { Dock = DockStyle.Top, Height = 175, Padding = new Padding(10,0,10,0) };
        var lblRep = new Label { Text = "3️⃣ Repuestos con Garantía (código + nombre + días) - opcional", Dock = DockStyle.Top, Height = 22, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.FromArgb(150,80,0) };
        var pRepInput = new Panel { Dock = DockStyle.Top, Height = 32 };
        var txtCod = new TextBox { PlaceholderText = "Código", Location = new Point(0, 3), Size = new Size(100, 26) };
        var txtNom = new TextBox { PlaceholderText = "Nombre repuesto", Location = new Point(105, 3), Size = new Size(200, 26) };
        var txtDias = new NumericUpDown { Minimum = 1, Maximum = 3650, Value = 90, Location = new Point(310, 3), Size = new Size(70, 26) };
        var lblDias = new Label { Text = "días", Location = new Point(385, 7), AutoSize = true };
        var btnAddRep = new Button { Text = "Agregar", Location = new Point(420, 3), Size = new Size(80, 26), BackColor = Color.FromArgb(30,60,110), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        var btnDelRep = new Button { Text = "Quitar", Location = new Point(505, 3), Size = new Size(80, 26), BackColor = Color.FromArgb(180,40,40), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        btnAddRep.Click += (s, e) => { if (string.IsNullOrWhiteSpace(txtCod.Text) || string.IsNullOrWhiteSpace(txtNom.Text)) { MessageBox.Show("Código y nombre requeridos"); return; } repuestos.Add((txtCod.Text.Trim(), txtNom.Text.Trim(), (int)txtDias.Value, DateTime.Now)); RefreshRepuestos(); txtCod.Clear(); txtNom.Clear(); };
        btnDelRep.Click += (s, e) => { if (dgvRepuestos.CurrentRow != null) { repuestos.RemoveAt(dgvRepuestos.CurrentRow.Index); RefreshRepuestos(); } };
        pRepInput.Controls.AddRange(new Control[] { txtCod, txtNom, txtDias, lblDias, btnAddRep, btnDelRep });
        dgvRepuestos.Dock = DockStyle.Fill;
        pRep.Controls.Add(dgvRepuestos); pRep.Controls.Add(pRepInput); pRep.Controls.Add(lblRep);

        // Bottom - altura aumentada a 105 para no cortar en DPI 125% y con AutoScroll
        var pBottom = new Panel { Dock = DockStyle.Bottom, Height = 105, BackColor = Color.FromArgb(245,245,245), Padding = new Padding(12) };
        txtObs.Location = new Point(12, 14); txtObs.Size = new Size(460, 30); txtObs.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblTotal.Location = new Point(485, 18); lblTotal.Text = "Total: $0.00"; lblTotal.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        var btnGuardar = new Button { Text = "💾 GUARDAR ORDEN", Location = new Point(720, 10), Size = new Size(190, 42), BackColor = Color.FromArgb(0,150,80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Anchor = AnchorStyles.Top | AnchorStyles.Right };
        var btnCancel = new Button { Text = "Cancelar", Location = new Point(720, 60), Size = new Size(190, 28), FlatStyle = FlatStyle.Flat, Anchor = AnchorStyles.Top | AnchorStyles.Right };
        btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;
        btnGuardar.Click += (s, e) => Guardar();
        pBottom.Controls.AddRange(new Control[] { txtObs, lblTotal, btnGuardar, btnCancel });

        // Orden de Dock: Bottom primero, luego Top en orden inverso para que no se tapen
        Controls.Add(pBottom);
        Controls.Add(pRep); Controls.Add(pSel); Controls.Add(pCat); Controls.Add(lblCat); Controls.Add(pTop);

        CargarVehiculos(); CargarCatalogo();
    }

    void CargarVehiculos()
    {
        var vehs = Repos.GetVehiculos();
        cmbPlaca.DataSource = vehs; cmbPlaca.DisplayMember = "Placa"; cmbPlaca.ValueMember = "Placa";
        if (vehs.Count > 0) cmbPlaca.SelectedIndex = 0;
        ActualizarCliente();
    }
    void ActualizarCliente()
    {
        if (cmbPlaca.SelectedItem is Vehiculo v) txtCliente.Text = $"{v.Cliente} - {v.Marca} {v.Modelo}";
    }
    void CargarCatalogo() { catalogo = Repos.GetServicios(); RefreshCatalogo(catalogo); RefreshSeleccion(); RefreshRepuestos(); }
    void FiltrarServicios(string f) { var q = string.IsNullOrWhiteSpace(f) ? catalogo : catalogo.Where(x => x.Codigo.Contains(f, StringComparison.OrdinalIgnoreCase) || x.Nombre.Contains(f, StringComparison.OrdinalIgnoreCase)).ToList(); RefreshCatalogo(q); }
    void RefreshCatalogo(List<Servicio> list) { dgvServicios.DataSource = list.Select(s => new { s.Id, s.Codigo, s.Nombre, Precio = s.Precio.ToString("C"), s.Descripcion }).ToList(); }
    void RefreshSeleccion()
    {
        dgvSeleccionados.DataSource = seleccion.Select((x, i) => new { Item = i + 1, x.s.Codigo, x.s.Nombre, Precio_Aplicado = x.precio.ToString("C"), x.cant, Subtotal = (x.precio * x.cant).ToString("C") }).ToList();
        lblTotal.Text = "Total: " + seleccion.Sum(x => x.precio * x.cant).ToString("C");
    }
    void RefreshRepuestos()
    {
        dgvRepuestos.DataSource = repuestos.Select((r, i) => new { Item = i + 1, r.codigo, r.nombre, Garantía = r.dias + " días", Vence = r.inicio.AddDays(r.dias).ToString("dd/MM/yyyy") }).ToList();
    }
    void AgregarServicio()
    {
        if (dgvServicios.CurrentRow == null) return;
        int id = Convert.ToInt32(dgvServicios.CurrentRow.Cells["Id"].Value);
        var s = catalogo.First(x => x.Id == id);
        using var d = new PrecioForm(s.Nombre, s.Precio);
        if (d.ShowDialog() != DialogResult.OK) return;
        seleccion.Add((s, d.Cantidad, d.Precio));
        RefreshSeleccion();
    }
    void QuitarServicio() { if (dgvSeleccionados.CurrentRow != null) { seleccion.RemoveAt(dgvSeleccionados.CurrentRow.Index); RefreshSeleccion(); } }
    void Guardar()
    {
        if (cmbPlaca.SelectedItem == null) { MessageBox.Show("Seleccione vehículo"); return; }
        if (seleccion.Count == 0) { MessageBox.Show("Agregue al menos un servicio"); return; }
        var placa = ((Vehiculo)cmbPlaca.SelectedItem).Placa;
        var orden = new Orden(0, placa, DateTime.Now, null, "Abierta", txtObs.Text, 0);
        var servs = seleccion.Select(x => (x.s.Id, x.s.Nombre, x.precio, x.cant)).ToList();
        var reps = repuestos.Select(r => (r.codigo, r.nombre, r.dias, r.inicio)).ToList();
        Repos.CreateOrden(orden, servs, reps);
        MessageBox.Show("Orden creada correctamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        DialogResult = DialogResult.OK;
    }
}

public class PrecioForm : Form
{
    public decimal Precio { get; private set; }
    public int Cantidad { get; private set; } = 1;
    public PrecioForm(string nombre, decimal precioActual)
    {
        Text = nombre; Size = new Size(320, 180); StartPosition = FormStartPosition.CenterParent; FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;
        var lbl = new Label { Text = "Precio a aplicar (editable):", Location = new Point(15, 15), AutoSize = true };
        var txtPrecio = new NumericUpDown { Location = new Point(15, 35), Size = new Size(270, 28), DecimalPlaces = 2, Maximum = 100000, Minimum = 0, Value = precioActual, Font = new Font("Segoe UI", 11) };
        var lblCant = new Label { Text = "Cantidad:", Location = new Point(15, 70), AutoSize = true };
        var txtCant = new NumericUpDown { Location = new Point(15, 90), Size = new Size(100, 28), Minimum = 1, Maximum = 100, Value = 1 };
        var btn = new Button { Text = "Agregar", Location = new Point(15, 125), Size = new Size(270, 30), BackColor = Color.FromArgb(0,150,80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        btn.Click += (s, e) => { Precio = txtPrecio.Value; Cantidad = (int)txtCant.Value; DialogResult = DialogResult.OK; };
        Controls.AddRange(new Control[] { lbl, txtPrecio, lblCant, txtCant, btn });
    }
}
