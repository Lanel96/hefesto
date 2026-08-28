using Hefesto.Core;

namespace Hefesto.Gui;

public class OrdenForm : Form
{
    ComboBox cmbPlaca = new() { DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat };
    TextBox txtCliente = new() { ReadOnly = true, BackColor = Color.FromArgb(240,240,240), BorderStyle = BorderStyle.FixedSingle };
    DataGridView dgvServicios = new() { AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle, RowHeadersVisible = false, AllowUserToResizeRows = false };
    DataGridView dgvSeleccionados = new() { AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle, RowHeadersVisible = false };
    DataGridView dgvRepuestos = new() { AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle, RowHeadersVisible = false };
    Label lblTotal = new() { Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.FromArgb(0,120,60), AutoSize = true, TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill };
    TextBox txtObs = new() { PlaceholderText = "Observaciones de la orden (máx 200 caracteres)...", MaxLength = 200, BorderStyle = BorderStyle.FixedSingle, Multiline = false };

    List<Servicio> catalogo = new();
    List<(Servicio s, int cant, decimal precio)> seleccion = new();
    List<(string codigo, string nombre, int dias, DateTime inicio)> repuestos = new();

    public OrdenForm()
    {
        Text = "➕ Nueva Orden - Seleccione vehículo y servicios";
        ClientSize = new Size(1020, 780);
        MinimumSize = new Size(980, 700);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9);
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoScaleDimensions = new SizeF(96F, 96F);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = true;
        DoubleBuffered = true;
        AutoScroll = true;

        // Layout principal - 6 filas escalables
        var main = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6, Padding = new Padding(8), BackColor = Color.White, AutoScroll = false };
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));  // pTop
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));  // lblCat
        main.RowStyles.Add(new RowStyle(SizeType.Percent, 33));   // pCat escalable
        main.RowStyles.Add(new RowStyle(SizeType.Percent, 33));   // pSel escalable
        main.RowStyles.Add(new RowStyle(SizeType.Percent, 34));   // pRep escalable
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 125)); // footer 125 - evita corte cancelar a 125% DPI

        // 1 - Vehículo
        var pTop = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(245,245,245), Padding = new Padding(10), Margin = new Padding(0,0,0,6) };
        var lblPlaca = new Label { Text = "Vehículo (Placa):", Dock = DockStyle.Top, Height = 18, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.FromArgb(30,60,110) };
        var tblTop = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, Padding = new Padding(0), Margin = new Padding(0) };
        tblTop.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
        tblTop.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
        tblTop.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145));
        tblTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        cmbPlaca.Dock = DockStyle.Fill; cmbPlaca.Margin = new Padding(0,0,4,0); cmbPlaca.Font = new Font("Segoe UI", 9.5F);
        var btnLupa = new Button { Text = "🔍", Dock = DockStyle.Fill, BackColor = Color.FromArgb(255, 193, 7), ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, Margin = new Padding(0,0,6,0), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
        btnLupa.FlatAppearance.BorderSize = 0;
        btnLupa.Click += (s, e) => { var placaActual = cmbPlaca.Text?.Trim(); using var f = new BuscarVehiculoForm(placaActual); if (f.ShowDialog() == DialogResult.OK && f.Seleccionado != null) { var vehs = Repos.GetVehiculos(); cmbPlaca.DataSource = vehs; cmbPlaca.DisplayMember = "Placa"; cmbPlaca.ValueMember = "Placa"; cmbPlaca.SelectedItem = vehs.FirstOrDefault(x => x.Placa == f.Seleccionado.Placa); ActualizarCliente(); } };
        var btnNuevoVeh = new Button { Text = "🚗 Nuevo Vehículo", Dock = DockStyle.Fill, BackColor = Color.FromArgb(30,60,110), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Margin = new Padding(0,0,8,0), Font = new Font("Segoe UI", 8, FontStyle.Bold) };
        btnNuevoVeh.FlatAppearance.BorderSize = 0;
        btnNuevoVeh.Click += (s, e) => { using var f = new VehiculoForm(null); if (f.ShowDialog() == DialogResult.OK) CargarVehiculos(); };
        txtCliente.Dock = DockStyle.Fill; txtCliente.Margin = new Padding(0); txtCliente.BorderStyle = BorderStyle.FixedSingle;
        txtCliente.Font = new Font("Segoe UI", 9.5F);
        txtCliente.PlaceholderText = "Cliente / Marca Modelo";
        cmbPlaca.SelectedIndexChanged += (s, e) => ActualizarCliente();
        tblTop.Controls.Add(cmbPlaca, 0, 0);
        tblTop.Controls.Add(btnLupa, 1, 0);
        tblTop.Controls.Add(btnNuevoVeh, 2, 0);
        tblTop.Controls.Add(txtCliente, 3, 0);
        pTop.Controls.Add(tblTop);
        pTop.Controls.Add(lblPlaca);

        // 2 - Catálogo label
        var lblCat = new Label { Text = "1️⃣ Catálogo de Servicios — doble clic para agregar (precio editable al agregar)", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.FromArgb(30,60,110), Padding = new Padding(6,0,0,0), BackColor = Color.FromArgb(235,240,255), Margin = new Padding(0,0,0,2) };

        // 3 - Catálogo grid
        var pCat = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0), Margin = new Padding(0) };
        var txtFiltro = new TextBox { PlaceholderText = "🔍 Filtrar servicios por código o nombre...", Dock = DockStyle.Top, Height = 30, Margin = new Padding(0,0,0,4), BorderStyle = BorderStyle.FixedSingle };
        txtFiltro.TextChanged += (s, e) => FiltrarServicios(txtFiltro.Text);
        dgvServicios.Dock = DockStyle.Fill;
        dgvServicios.DoubleClick += (s, e) => AgregarServicio();
        dgvServicios.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.Handled = true; e.SuppressKeyPress = true; AgregarServicio(); } };
        // placeholder para empty
        pCat.Controls.Add(dgvServicios); pCat.Controls.Add(txtFiltro);

        // 4 - Seleccionados
        var pSel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0), Margin = new Padding(0) };
        var lblSel = new Label { Text = "2️⃣ Servicios Seleccionados — precio congelado para esta orden", Dock = DockStyle.Top, Height = 22, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.FromArgb(0,120,60), Padding = new Padding(6,2,0,0), BackColor = Color.FromArgb(235,255,235) };
        var btnQuitar = new Button { Text = "Quitar seleccionado", Dock = DockStyle.Bottom, Height = 30, BackColor = Color.FromArgb(180,40,40), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8) };
        btnQuitar.FlatAppearance.BorderSize = 0;
        btnQuitar.Click += (s, e) => QuitarServicio();
        dgvSeleccionados.Dock = DockStyle.Fill;
        pSel.Controls.Add(dgvSeleccionados); pSel.Controls.Add(btnQuitar); pSel.Controls.Add(lblSel);

        // 5 - Repuestos
        var pRep = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0), Margin = new Padding(0) };
        var lblRep = new Label { Text = "3️⃣ Repuestos con Garantía — código + nombre + días (opcional)", Dock = DockStyle.Top, Height = 22, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.FromArgb(150,80,0), Padding = new Padding(6,2,0,0), BackColor = Color.FromArgb(255,245,220) };
        var pRepInput = new TableLayoutPanel { Dock = DockStyle.Top, Height = 40, ColumnCount = 7, RowCount = 1, Padding = new Padding(0,4,0,4), BackColor = Color.Transparent };
        pRepInput.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115));
        pRepInput.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 36));
        pRepInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        pRepInput.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
        pRepInput.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 55));
        pRepInput.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        pRepInput.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        var txtCod = new TextBox { PlaceholderText = "Código", Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0,0,4,0) };
        var txtNom = new TextBox { PlaceholderText = "Nombre repuesto", Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0,0,4,0) };
        var btnBuscarInv = new Button { Text = "🔍", Dock = DockStyle.Fill, BackColor = Color.FromArgb(255,193,7), ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, Margin = new Padding(0,0,4,0), Font = new Font("Segoe UI", 8, FontStyle.Bold) };
        btnBuscarInv.FlatAppearance.BorderSize = 0;
        btnBuscarInv.Click += (s, e) => { using var f = new BuscarInventarioForm(); if (f.ShowDialog() == DialogResult.OK && f.Seleccionado != null) { txtCod.Text = f.Seleccionado.Codigo; txtNom.Text = f.Seleccionado.Nombre; } };
        var txtDias = new NumericUpDown { Minimum = 1, Maximum = 3650, Value = 90, Dock = DockStyle.Fill, Margin = new Padding(0,0,4,0), BorderStyle = BorderStyle.FixedSingle, TextAlign = HorizontalAlignment.Center };
        var lblDias = new Label { Text = "días", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoSize = false };
        var btnAddRep = new Button { Text = "Agregar", Dock = DockStyle.Fill, BackColor = Color.FromArgb(30,60,110), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Margin = new Padding(0,0,6,0), Font = new Font("Segoe UI", 8, FontStyle.Bold) };
        var btnDelRep = new Button { Text = "Quitar", Dock = DockStyle.Fill, BackColor = Color.FromArgb(180,40,40), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Margin = new Padding(0), Font = new Font("Segoe UI", 8) };
        btnAddRep.FlatAppearance.BorderSize = 0; btnDelRep.FlatAppearance.BorderSize = 0;
        btnAddRep.Click += (s, e) => { if (string.IsNullOrWhiteSpace(txtCod.Text) || string.IsNullOrWhiteSpace(txtNom.Text)) { MessageBox.Show("Código y nombre requeridos"); return; } repuestos.Add((txtCod.Text.Trim(), txtNom.Text.Trim(), (int)txtDias.Value, DateTime.Now)); RefreshRepuestos(); txtCod.Clear(); txtNom.Clear(); };
        btnDelRep.Click += (s, e) => { if (dgvRepuestos.CurrentRow != null) { repuestos.RemoveAt(dgvRepuestos.CurrentRow.Index); RefreshRepuestos(); } };
        pRepInput.Controls.Add(txtCod, 0, 0); pRepInput.Controls.Add(btnBuscarInv, 1, 0); pRepInput.Controls.Add(txtNom, 2, 0); pRepInput.Controls.Add(txtDias, 3, 0); pRepInput.Controls.Add(lblDias, 4, 0); pRepInput.Controls.Add(btnAddRep, 5, 0); pRepInput.Controls.Add(btnDelRep, 6, 0);
        txtCod.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { var inv = Repos.GetInventarioByCodigo(txtCod.Text.Trim()); if (inv != null) { txtNom.Text = inv.Nombre; txtDias.Focus(); } e.Handled = true; e.SuppressKeyPress = true; } };
        dgvRepuestos.Dock = DockStyle.Fill;
        pRep.Controls.Add(dgvRepuestos); pRep.Controls.Add(pRepInput); pRep.Controls.Add(lblRep);

        // 6 - Footer - TableLayout proporcionado 50% obs | 15% total | 35% botones
        var pBottom = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, BackColor = Color.FromArgb(245,245,245), Padding = new Padding(10), Margin = new Padding(0,6,0,0) };
        pBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52));
        pBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18));
        pBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        var pObsWrap = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0), Margin = new Padding(0,0,8,0), BackColor = Color.Transparent };
        var lblObs = new Label { Text = "Observaciones:", Dock = DockStyle.Top, Height = 16, Font = new Font("Segoe UI", 7, FontStyle.Bold), ForeColor = Color.Gray };
        txtObs.Dock = DockStyle.Fill; txtObs.Margin = new Padding(0,2,0,0);
        var lblObsHint = new Label { Text = "0/200", Dock = DockStyle.Bottom, Height = 14, Font = new Font("Segoe UI", 6), ForeColor = Color.Gray, TextAlign = ContentAlignment.MiddleRight };
        txtObs.TextChanged += (s, e) => lblObsHint.Text = $"{txtObs.Text.Length}/200";
        pObsWrap.Controls.Add(txtObs); pObsWrap.Controls.Add(lblObsHint); pObsWrap.Controls.Add(lblObs);
        // total centrado
        var pTotalWrap = new Panel { Dock = DockStyle.Fill, Padding = new Padding(4), BackColor = Color.Transparent };
        lblTotal.Dock = DockStyle.Fill; lblTotal.TextAlign = ContentAlignment.MiddleCenter; lblTotal.Text = "Total: $0.00";
        pTotalWrap.Controls.Add(lblTotal);
        // botones
        var pBtns = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(0), Margin = new Padding(0) };
        pBtns.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
        pBtns.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
        var btnGuardar = new Button { Text = "💾 GUARDAR ORDEN", Dock = DockStyle.Fill, BackColor = Color.FromArgb(0,150,80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Margin = new Padding(0,0,0,4) };
        var btnCancel = new Button { Text = "Cancelar", Dock = DockStyle.Fill, FlatStyle = FlatStyle.Flat, Margin = new Padding(0,4,0,0) };
        btnGuardar.FlatAppearance.BorderSize = 0;
        btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;
        btnGuardar.Click += (s, e) => Guardar();
        pBtns.Controls.Add(btnGuardar, 0, 0);
        pBtns.Controls.Add(btnCancel, 0, 1);
        pBottom.Controls.Add(pObsWrap, 0, 0);
        pBottom.Controls.Add(pTotalWrap, 1, 0);
        pBottom.Controls.Add(pBtns, 2, 0);

        main.Controls.Add(pTop, 0, 0);
        main.Controls.Add(lblCat, 0, 1);
        main.Controls.Add(pCat, 0, 2);
        main.Controls.Add(pSel, 0, 3);
        main.Controls.Add(pRep, 0, 4);
        main.Controls.Add(pBottom, 0, 5);

        Controls.Add(main);

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
        Text = nombre; ClientSize = new Size(360, 220); MinimumSize = new Size(380, 250); StartPosition = FormStartPosition.CenterParent; FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false; AutoScaleMode = AutoScaleMode.Dpi; BackColor = Color.White;
        var lbl = new Label { Text = "Precio a aplicar (editable):", Location = new Point(15, 15), AutoSize = true, Font = new Font("Segoe UI", 8, FontStyle.Bold) };
        var txtPrecio = new NumericUpDown { Location = new Point(15, 35), Size = new Size(330, 28), DecimalPlaces = 2, Maximum = 100000, Minimum = 0, Value = precioActual, Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
        var lblCant = new Label { Text = "Cantidad:", Location = new Point(15, 75), AutoSize = true, Font = new Font("Segoe UI", 8, FontStyle.Bold) };
        var txtCant = new NumericUpDown { Location = new Point(15, 95), Size = new Size(120, 28), Minimum = 1, Maximum = 100, Value = 1, BorderStyle = BorderStyle.FixedSingle };
        var btn = new Button { Text = "Agregar", Location = new Point(15, 145), Size = new Size(330, 36), BackColor = Color.FromArgb(0,150,80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
        btn.FlatAppearance.BorderSize = 0;
        btn.Click += (s, e) => { Precio = txtPrecio.Value; Cantidad = (int)txtCant.Value; DialogResult = DialogResult.OK; };
        Controls.AddRange(new Control[] { lbl, txtPrecio, lblCant, txtCant, btn });
    }
}
