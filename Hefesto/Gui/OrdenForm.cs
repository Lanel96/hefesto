using System.Windows.Forms;
using Hefesto.Core;

namespace Hefesto.Gui;

public enum TipoConcepto { Servicio, Repuesto, Manual }

public class ConceptoOrden
{
    public TipoConcepto Tipo { get; set; }
    public string TipoStr => Tipo == TipoConcepto.Servicio ? "Servicio" : Tipo == TipoConcepto.Repuesto ? "Repuesto" : "Manual";
    public string TipoIcon => Tipo == TipoConcepto.Servicio ? "🔧" : Tipo == TipoConcepto.Repuesto ? "📦" : "✏️";
    public int? ServicioId { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public decimal Precio { get; set; }
    public int Cantidad { get; set; } = 1;
    public int DiasGarantia { get; set; } = 0;
    public DateTime FechaInicio { get; set; } = DateTime.Now;
    public string GarantiaStr => DiasGarantia > 0 ? $"{DiasGarantia} días" : "—";
}

public class OrdenForm : Form
{
    readonly ToolTip toolTip = new();
    ComboBox cmbPlaca = new() { DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat };
    TextBox txtCliente = new() { ReadOnly = true, BackColor = Color.FromArgb(240, 240, 240), BorderStyle = BorderStyle.FixedSingle };
    DataGridView dgvConceptos = new()
    {
        AllowUserToAddRows = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        Dock = DockStyle.Fill,
        BackgroundColor = Color.White,
        BorderStyle = BorderStyle.FixedSingle,
        RowHeadersVisible = false,
        AllowUserToResizeRows = false,
        MultiSelect = false
    };
    Label lblTotal = new() { Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.FromArgb(0, 120, 60), AutoSize = true, TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill };
    TextBox txtObs = new() { PlaceholderText = "Observaciones de la orden (máx 200 caracteres)...", MaxLength = 200, BorderStyle = BorderStyle.FixedSingle, Multiline = false };

    List<Servicio> catalogoServicios = new();
    List<InventarioItem> catalogoInventario = new();
    List<ConceptoOrden> conceptos = new();

    public OrdenForm()
    {
        Text = "➕ Nueva Orden de Trabajo";
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9);
        AutoScaleMode = AutoScaleMode.None; // Control manual total
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = true;
        DoubleBuffered = true;

        // DPI real
        float dpiScale = 1f;
        using (var g = CreateGraphics()) dpiScale = g.DpiX / 96f;

        // Tamaños base (96 DPI) escalados
        int baseW = 1000, baseH = 720;
        int minW = 950, minH = 680;
        ClientSize = new Size((int)(baseW * dpiScale), (int)(baseH * dpiScale));
        MinimumSize = new Size((int)(minW * dpiScale), (int)(minH * dpiScale));

        int hVehicle = (int)Math.Round(85 * dpiScale);
        int hToolbar = (int)Math.Round(50 * dpiScale);
        int hTotalBar = (int)Math.Round(45 * dpiScale);
        int hObs = (int)Math.Round(70 * dpiScale);
        int hBtns = (int)Math.Round(100 * dpiScale); // más alto para botones

        var main = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            Padding = new Padding((int)(12 * dpiScale)),
            BackColor = Color.White
        };
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, hVehicle));
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, hToolbar));
        main.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, hTotalBar));
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, hObs));
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, hBtns));

        // ===== ROW 0: VEHICLE SELECTOR =====
        var pVehicle = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(248, 249, 250), Padding = new Padding(12, 10, 12, 10), Margin = new Padding(0, 0, 0, 8) };
        pVehicle.Paint += (s, e) => { using var pen = new Pen(Color.FromArgb(220, 220, 220)); e.Graphics.DrawLine(pen, 0, pVehicle.Height - 1, pVehicle.Width, pVehicle.Height - 1); };

        var lblVehicle = new Label { Text = "VEHÍCULO", Dock = DockStyle.Top, Height = 18, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.FromArgb(80, 80, 80), Padding = new Padding(0, 0, 0, 4) };
        var tblVehicle = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, Padding = new Padding(0), Margin = new Padding(0) };
        tblVehicle.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
        tblVehicle.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 44));
        tblVehicle.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        tblVehicle.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        cmbPlaca.Dock = DockStyle.Fill; cmbPlaca.Margin = new Padding(0, 0, 4, 0); cmbPlaca.Font = new Font("Segoe UI", 10);
        cmbPlaca.FlatStyle = FlatStyle.Flat;

        var btnBuscarVeh = new Button { Text = "🔍", Dock = DockStyle.Fill, BackColor = Color.FromArgb(255, 193, 7), ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 0, 6, 0), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
        toolTip.SetToolTip(btnBuscarVeh, "Buscar vehículo (F2)");
        btnBuscarVeh.FlatAppearance.BorderSize = 0;
        btnBuscarVeh.Click += (s, e) => BuscarVehiculo();

        var btnNuevoVeh = new Button { Text = "+ Nuevo", Dock = DockStyle.Fill, BackColor = Color.FromArgb(30, 60, 110), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 0, 8, 0), Font = new Font("Segoe UI", 8, FontStyle.Bold) };
        toolTip.SetToolTip(btnNuevoVeh, "Registrar nuevo vehículo (F3)");
        btnNuevoVeh.FlatAppearance.BorderSize = 0;
        btnNuevoVeh.Click += (s, e) => { using var f = new VehiculoForm(null); if (f.ShowDialog() == DialogResult.OK) CargarVehiculos(); };

        txtCliente.Dock = DockStyle.Fill; txtCliente.Margin = new Padding(0); txtCliente.BorderStyle = BorderStyle.FixedSingle;
        txtCliente.Font = new Font("Segoe UI", 10);
        txtCliente.PlaceholderText = "Cliente • Marca Modelo";
        cmbPlaca.SelectedIndexChanged += (s, e) => ActualizarCliente();

        tblVehicle.Controls.Add(cmbPlaca, 0, 0);
        tblVehicle.Controls.Add(btnBuscarVeh, 1, 0);
        tblVehicle.Controls.Add(btnNuevoVeh, 2, 0);
        tblVehicle.Controls.Add(txtCliente, 3, 0);
        pVehicle.Controls.Add(tblVehicle);
        pVehicle.Controls.Add(lblVehicle);

        // ===== ROW 1: TOOLBAR =====
        var pToolbar = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(0, 4, 0, 4) };
        var tblToolbar = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 5, RowCount = 1, Padding = new Padding(0), Margin = new Padding(0) };
        tblToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        tblToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        tblToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        tblToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        tblToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));

        var btnAddServicio = CreateToolbarButton("🔧  Servicio", Color.FromArgb(30, 60, 110), "Agregar servicio desde catálogo (F4)", () => AgregarConcepto(TipoConcepto.Servicio));
        var btnAddRepuesto = CreateToolbarButton("📦  Repuesto", Color.FromArgb(150, 80, 0), "Agregar repuesto desde inventario (F5)", () => AgregarConcepto(TipoConcepto.Repuesto));
        var btnAddManual = CreateToolbarButton("✏️  Manual", Color.FromArgb(80, 80, 80), "Agregar concepto manual (F6)", () => AgregarConcepto(TipoConcepto.Manual));

        var txtFiltro = new TextBox { PlaceholderText = "🔍  Filtrar conceptos...", Dock = DockStyle.Fill, Margin = new Padding(8, 8, 8, 8), BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 9.5F) };
        txtFiltro.TextChanged += (s, e) => FiltrarConceptos(txtFiltro.Text);

        var btnQuitar = new Button { Text = "🗑  Quitar", Dock = DockStyle.Fill, Margin = new Padding(4, 8, 4, 8), BackColor = Color.FromArgb(180, 40, 40), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
        toolTip.SetToolTip(btnQuitar, "Quitar concepto seleccionado (Supr)");
        btnQuitar.FlatAppearance.BorderSize = 0;
        btnQuitar.Click += (s, e) => QuitarConcepto();

        var btnDuplicar = new Button { Text = "📋  Duplicar", Dock = DockStyle.Fill, Margin = new Padding(4, 8, 0, 8), BackColor = Color.FromArgb(100, 100, 100), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
        toolTip.SetToolTip(btnDuplicar, "Duplicar concepto seleccionado");
        btnDuplicar.FlatAppearance.BorderSize = 0;
        btnDuplicar.Click += (s, e) => DuplicarConcepto();

        tblToolbar.Controls.Add(btnAddServicio, 0, 0);
        tblToolbar.Controls.Add(btnAddRepuesto, 1, 0);
        tblToolbar.Controls.Add(txtFiltro, 2, 0);
        tblToolbar.Controls.Add(btnQuitar, 3, 0);
        tblToolbar.Controls.Add(btnDuplicar, 4, 0);
        pToolbar.Controls.Add(tblToolbar);

        // ===== ROW 2: CONCEPTOS GRID =====
        var pGrid = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0), Margin = new Padding(0) };
        ConfigurarGridConceptos();
        dgvConceptos.Dock = DockStyle.Fill;
        dgvConceptos.DoubleClick += (s, e) => EditarConcepto();
        dgvConceptos.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Delete) { e.Handled = true; QuitarConcepto(); }
            else if (e.KeyCode == Keys.Enter) { e.Handled = true; e.SuppressKeyPress = true; EditarConcepto(); }
            else if (e.KeyCode == Keys.F4) { e.Handled = true; AgregarConcepto(TipoConcepto.Servicio); }
            else if (e.KeyCode == Keys.F5) { e.Handled = true; AgregarConcepto(TipoConcepto.Repuesto); }
            else if (e.KeyCode == Keys.F6) { e.Handled = true; AgregarConcepto(TipoConcepto.Manual); }
            else if (e.KeyCode == Keys.F2) { e.Handled = true; BuscarVehiculo(); }
            else if (e.KeyCode == Keys.F3) { e.Handled = true; using var f = new VehiculoForm(null); if (f.ShowDialog() == DialogResult.OK) CargarVehiculos(); }
        };
        pGrid.Controls.Add(dgvConceptos);

        // ===== ROW 3: TOTAL BAR =====
        var pTotalBar = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(240, 245, 250), Padding = new Padding(16, 0, 16, 0) };
        pTotalBar.Paint += (s, e) => { using var pen = new Pen(Color.FromArgb(200, 210, 220)); e.Graphics.DrawLine(pen, 0, 0, pTotalBar.Width, 0); };
        var lblTotalBar = new Label { Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.FromArgb(0, 120, 60), AutoSize = true, TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill, Text = "Total: $0.00" };
        pTotalBar.Controls.Add(lblTotalBar);

        // ===== ROW 4: OBSERVACIONES =====
        var pObsRow = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Color.FromArgb(248, 249, 250), Padding = new Padding(12, 8, 12, 8), Margin = new Padding(0, 8, 0, 0) };
        pObsRow.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
        pObsRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var lblObs = new Label { Text = "OBSERVACIONES", Dock = DockStyle.Top, Height = 16, Font = new Font("Segoe UI", 7, FontStyle.Bold), ForeColor = Color.Gray };
        var pObsWrap = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 8, 0), BackColor = Color.Transparent };
        txtObs.Dock = DockStyle.Fill; txtObs.Margin = new Padding(0, 2, 0, 0); txtObs.Font = new Font("Segoe UI", 9.5F);
        var lblObsHint = new Label { Text = "0/200", Dock = DockStyle.Bottom, Height = 14, Font = new Font("Segoe UI", 6), ForeColor = Color.Gray, TextAlign = ContentAlignment.MiddleRight };
        txtObs.TextChanged += (s, e) => lblObsHint.Text = $"{txtObs.Text.Length}/200";
        pObsWrap.Controls.Add(txtObs); pObsWrap.Controls.Add(lblObsHint); pObsWrap.Controls.Add(lblObs);
        pObsRow.Controls.Add(lblObs, 0, 0);
        pObsRow.Controls.Add(pObsWrap, 0, 1);

        // ===== ROW 5: BOTONES (altura fija, nunca se corta) =====
        int btnH1 = (int)Math.Round(52 * dpiScale);
        int btnH2 = (int)Math.Round(40 * dpiScale);
        
        var pBtnsRow = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(248, 249, 250), Padding = new Padding((int)(12 * dpiScale), (int)(8 * dpiScale), (int)(12 * dpiScale), (int)(8 * dpiScale)) };
        var pBtns = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, BackColor = Color.Transparent };
        pBtns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
        pBtns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
        pBtns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        
        var pBtnsInner = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(0), Margin = new Padding(0) };
        pBtnsInner.RowStyles.Add(new RowStyle(SizeType.Absolute, btnH1));
        pBtnsInner.RowStyles.Add(new RowStyle(SizeType.Absolute, btnH2));
        var btnGuardar = new Button { Text = "💾  GUARDAR ORDEN", Dock = DockStyle.Fill, BackColor = Color.FromArgb(0, 150, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Margin = new Padding(0, 0, 0, 4) };
        var btnCancel = new Button { Text = "Cancelar", Dock = DockStyle.Fill, FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 4, 0, 0), Font = new Font("Segoe UI", 9) };
        btnGuardar.FlatAppearance.BorderSize = 0;
        btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;
        btnGuardar.Click += (s, e) => Guardar();
        pBtnsInner.Controls.Add(btnGuardar, 0, 0);
        pBtnsInner.Controls.Add(btnCancel, 0, 1);
        
        pBtns.Controls.Add(new Panel { Dock = DockStyle.Fill }, 0, 0); // spacer
        pBtns.Controls.Add(new Panel { Dock = DockStyle.Fill }, 1, 0); // spacer
        pBtns.Controls.Add(pBtnsInner, 2, 0);
        pBtnsRow.Controls.Add(pBtns);

        main.Controls.Add(pVehicle, 0, 0);
        main.Controls.Add(pToolbar, 0, 1);
        main.Controls.Add(pGrid, 0, 2);
        main.Controls.Add(pTotalBar, 0, 3);
        main.Controls.Add(pObsRow, 0, 4);
        main.Controls.Add(pBtnsRow, 0, 5);

        // Update lblTotal reference to the new label in total bar
        lblTotal = lblTotalBar;

        Controls.Add(main);
        KeyPreview = true;
        KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) DialogResult = DialogResult.Cancel; };

        CargarVehiculos();
        CargarCatalogos();
    }

    Button CreateToolbarButton(string text, Color backColor, string tooltip, Action onClick)
    {
        var btn = new Button
        {
            Text = text,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 8, 6, 8),
            BackColor = backColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        toolTip.SetToolTip(btn, tooltip);
        btn.FlatAppearance.BorderSize = 0;
        btn.Click += (s, e) => onClick();
        return btn;
    }

    void ConfigurarGridConceptos()
    {
        dgvConceptos.Columns.Clear();
        dgvConceptos.Columns.Add(new DataGridViewTextBoxColumn { Name = "Tipo", HeaderText = "Tipo", DataPropertyName = "TipoIcon", FillWeight = 8, ReadOnly = true });
        dgvConceptos.Columns.Add(new DataGridViewTextBoxColumn { Name = "Codigo", HeaderText = "Código", DataPropertyName = "Codigo", FillWeight = 12, ReadOnly = true });
        dgvConceptos.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nombre", HeaderText = "Concepto / Descripción", DataPropertyName = "Nombre", FillWeight = 35, ReadOnly = true });
        dgvConceptos.Columns.Add(new DataGridViewTextBoxColumn { Name = "Precio", HeaderText = "Precio Unit.", DataPropertyName = "PrecioStr", FillWeight = 12, ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight, Format = "C" } });
        dgvConceptos.Columns.Add(new DataGridViewTextBoxColumn { Name = "Cant", HeaderText = "Cant.", DataPropertyName = "Cantidad", FillWeight = 8, ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
        dgvConceptos.Columns.Add(new DataGridViewTextBoxColumn { Name = "Subtotal", HeaderText = "Subtotal", DataPropertyName = "SubtotalStr", FillWeight = 12, ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight, Format = "C", Font = new Font("Segoe UI", 9, FontStyle.Bold) } });
        dgvConceptos.Columns.Add(new DataGridViewTextBoxColumn { Name = "Garantia", HeaderText = "Garantía", DataPropertyName = "GarantiaStr", FillWeight = 13, ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });

        dgvConceptos.CellFormatting += (s, e) =>
        {
            if (e.RowIndex >= 0 && e.RowIndex < conceptos.Count)
            {
                var c = conceptos[e.RowIndex];
                if (e.ColumnIndex == 0) { e.Value = c.Tipo == TipoConcepto.Servicio ? "🔧" : c.Tipo == TipoConcepto.Repuesto ? "📦" : "✏️"; }
                else if (e.ColumnIndex == 3) { e.Value = c.Precio.ToString("C"); e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleRight; }
                else if (e.ColumnIndex == 5) { e.Value = (c.Precio * c.Cantidad).ToString("C"); e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleRight; e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold); }
                else if (e.ColumnIndex == 6)
                {
                    if (c.Tipo == TipoConcepto.Repuesto && c.DiasGarantia > 0)
                    {
                        var venc = c.FechaInicio.AddDays(c.DiasGarantia);
                        var dias = (venc.Date - DateTime.Now.Date).Days;
                        e.Value = dias > 0 ? $"{c.DiasGarantia} días ({dias} restantes)" : $"{c.DiasGarantia} días (VENCIDA)";
                        e.CellStyle.ForeColor = dias > 0 ? Color.FromArgb(0, 120, 60) : Color.FromArgb(180, 40, 40);
                    }
                    else e.Value = "—";
                }
                else e.Value = e.Value ?? "";
            }
        };

        dgvConceptos.RowPrePaint += (s, e) =>
        {
            if (e.RowIndex >= 0 && e.RowIndex < conceptos.Count)
            {
                var c = conceptos[e.RowIndex];
                if (c.Tipo == TipoConcepto.Servicio) dgvConceptos.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(235, 242, 255);
                else if (c.Tipo == TipoConcepto.Repuesto) dgvConceptos.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 248, 230);
                else dgvConceptos.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            }
        };
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
        if (cmbPlaca.SelectedItem is Vehiculo v) txtCliente.Text = $"{v.Cliente}  •  {v.Marca} {v.Modelo}";
    }

    void CargarCatalogos()
    {
        catalogoServicios = Repos.GetServicios();
        catalogoInventario = Repos.GetInventario();
        RefreshGrid();
    }

    void FiltrarConceptos(string filtro)
    {
        if (string.IsNullOrWhiteSpace(filtro)) { RefreshGrid(); return; }
        var f = filtro.ToLower();
        var filtered = conceptos.Where(c =>
            c.Codigo.ToLower().Contains(f) ||
            c.Nombre.ToLower().Contains(f) ||
            c.TipoStr.ToLower().Contains(f)
        ).ToList();
        dgvConceptos.DataSource = filtered.Select(c => new { c.TipoIcon, c.Codigo, c.Nombre, PrecioStr = c.Precio.ToString("C"), c.Cantidad, SubtotalStr = (c.Precio * c.Cantidad).ToString("C"), c.GarantiaStr }).ToList();
    }

    void RefreshGrid()
    {
        dgvConceptos.DataSource = conceptos.Select(c => new { c.TipoIcon, c.Codigo, c.Nombre, PrecioStr = c.Precio.ToString("C"), c.Cantidad, SubtotalStr = (c.Precio * c.Cantidad).ToString("C"), c.GarantiaStr }).ToList();
        ActualizarTotal();
    }

    void ActualizarTotal()
    {
        var total = conceptos.Sum(c => c.Precio * c.Cantidad);
        lblTotal.Text = $"Total: {total:C}";
    }

    void AgregarConcepto(TipoConcepto tipo)
    {
        using var dlg = new ConceptoForm(tipo, catalogoServicios, catalogoInventario);
        if (dlg.ShowDialog() == DialogResult.OK && dlg.Concepto != null)
        {
            conceptos.Add(dlg.Concepto);
            RefreshGrid();
        }
    }

    void EditarConcepto()
    {
        if (dgvConceptos.CurrentRow == null) return;
        int idx = dgvConceptos.CurrentRow.Index;
        if (idx < 0 || idx >= conceptos.Count) return;

        var concepto = conceptos[idx];
        using var dlg = new ConceptoForm(concepto.Tipo, catalogoServicios, catalogoInventario, concepto);
        if (dlg.ShowDialog() == DialogResult.OK && dlg.Concepto != null)
        {
            conceptos[idx] = dlg.Concepto;
            RefreshGrid();
        }
    }

    void QuitarConcepto()
    {
        if (dgvConceptos.CurrentRow == null) return;
        int idx = dgvConceptos.CurrentRow.Index;
        if (idx >= 0 && idx < conceptos.Count)
        {
            conceptos.RemoveAt(idx);
            RefreshGrid();
        }
    }

    void DuplicarConcepto()
    {
        if (dgvConceptos.CurrentRow == null) return;
        int idx = dgvConceptos.CurrentRow.Index;
        if (idx >= 0 && idx < conceptos.Count)
        {
            var original = conceptos[idx];
            var copia = new ConceptoOrden
            {
                Tipo = original.Tipo,
                ServicioId = original.ServicioId,
                Codigo = original.Codigo,
                Nombre = original.Nombre,
                Precio = original.Precio,
                Cantidad = original.Cantidad,
                DiasGarantia = original.DiasGarantia,
                FechaInicio = DateTime.Now
            };
            conceptos.Insert(idx + 1, copia);
            RefreshGrid();
        }
    }

    void BuscarVehiculo()
    {
        var placaActual = cmbPlaca.Text?.Trim();
        using var f = new BuscarVehiculoForm(placaActual);
        if (f.ShowDialog() == DialogResult.OK && f.Seleccionado != null)
        {
            var vehs = Repos.GetVehiculos();
            cmbPlaca.DataSource = vehs;
            cmbPlaca.DisplayMember = "Placa";
            cmbPlaca.ValueMember = "Placa";
            cmbPlaca.SelectedItem = vehs.FirstOrDefault(x => x.Placa == f.Seleccionado.Placa);
            ActualizarCliente();
        }
    }

    void Guardar()
    {
        if (cmbPlaca.SelectedItem == null) { MessageBox.Show("Seleccione un vehículo", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        if (conceptos.Count == 0) { MessageBox.Show("Agregue al menos un concepto (servicio o repuesto)", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

        var placa = ((Vehiculo)cmbPlaca.SelectedItem).Placa;
        var orden = new Orden(0, placa, DateTime.Now, null, "Abierta", txtObs.Text, 0);

        var servs = conceptos
            .Where(c => c.Tipo == TipoConcepto.Servicio && c.ServicioId.HasValue)
            .Select(c => (c.ServicioId!.Value, c.Nombre, c.Precio, c.Cantidad))
            .ToList();

        var reps = conceptos
            .Where(c => c.Tipo == TipoConcepto.Repuesto || c.Tipo == TipoConcepto.Manual)
            .Select(c => (c.Codigo, c.Nombre, c.DiasGarantia, c.FechaInicio))
            .ToList();

        Repos.CreateOrden(orden, servs, reps);
        MessageBox.Show("Orden creada correctamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        DialogResult = DialogResult.OK;
    }
}

public class ConceptoForm : Form
{
    readonly ToolTip toolTip = new();
    public ConceptoOrden? Concepto { get; private set; }
    readonly TipoConcepto _tipo;
    readonly List<Servicio> _servicios;
    readonly List<InventarioItem> _inventario;
    readonly ConceptoOrden? _editando;

    ComboBox cmbTipo = new() { DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, Dock = DockStyle.Fill };
    TextBox txtCodigo = new() { CharacterCasing = CharacterCasing.Upper, BorderStyle = BorderStyle.FixedSingle, Dock = DockStyle.Fill, PlaceholderText = "Código" };
    TextBox txtNombre = new() { BorderStyle = BorderStyle.FixedSingle, Dock = DockStyle.Fill, PlaceholderText = "Nombre / Descripción" };
    NumericUpDown txtPrecio = new() { DecimalPlaces = 2, Maximum = 1000000, Minimum = 0, BorderStyle = BorderStyle.FixedSingle, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11) };
    NumericUpDown txtCantidad = new() { Minimum = 1, Maximum = 1000, Value = 1, BorderStyle = BorderStyle.FixedSingle, Dock = DockStyle.Fill, TextAlign = HorizontalAlignment.Center };
    NumericUpDown txtDiasGarantia = new() { Minimum = 0, Maximum = 3650, Value = 90, BorderStyle = BorderStyle.FixedSingle, Dock = DockStyle.Fill, TextAlign = HorizontalAlignment.Center };
    DateTimePicker dtpInicio = new() { Format = DateTimePickerFormat.Short, Dock = DockStyle.Fill };
    Button btnBuscarCatalogo = new() { Text = "🔍", Dock = DockStyle.Fill, BackColor = Color.FromArgb(255, 193, 7), ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Width = 40 };

    public ConceptoForm(TipoConcepto tipo, List<Servicio> servicios, List<InventarioItem> inventario, ConceptoOrden? editando = null)
    {
        _tipo = tipo;
        _servicios = servicios;
        _inventario = inventario;
        _editando = editando;

        Text = editando == null ? "Agregar Concepto" : "Editar Concepto";
        ClientSize = new Size(520, 420);
        MinimumSize = new Size(520, 420);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.White;
        KeyPreview = true;

        var main = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 8, Padding = new Padding(16), BackColor = Color.White };
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        main.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

        // Tipo selector (solo si es nuevo)
        if (editando == null)
        {
            var pTipo = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Padding = new Padding(0, 8, 0, 0) };
            pTipo.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            pTipo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            pTipo.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 0));
            var lblTipo = new Label { Text = "TIPO *", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.FromArgb(80, 80, 80) };
            cmbTipo.Items.AddRange(new object[] { "🔧  Servicio", "📦  Repuesto", "✏️  Manual" });
            cmbTipo.SelectedIndex = tipo == TipoConcepto.Servicio ? 0 : tipo == TipoConcepto.Repuesto ? 1 : 2;
            cmbTipo.SelectedIndexChanged += (s, e) => ActualizarCamposPorTipo();
            pTipo.Controls.Add(lblTipo, 0, 0);
            pTipo.Controls.Add(cmbTipo, 1, 0);
            main.Controls.Add(pTipo, 0, 0);
        }
        else
        {
            var pHidden = new Panel { Dock = DockStyle.Fill, Height = 0, Visible = false };
            main.Controls.Add(pHidden, 0, 0);
        }

        // Código + Buscar
        var pCodigo = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Padding = new Padding(0, 8, 0, 0) };
        pCodigo.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        pCodigo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        pCodigo.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48));
        var lblCod = new Label { Text = "CÓDIGO *", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.FromArgb(80, 80, 80) };
        btnBuscarCatalogo.FlatAppearance.BorderSize = 0;
        btnBuscarCatalogo.Click += (s, e) => BuscarEnCatalogo();
        pCodigo.Controls.Add(lblCod, 0, 0);
        pCodigo.Controls.Add(txtCodigo, 1, 0);
        pCodigo.Controls.Add(btnBuscarCatalogo, 2, 0);
        main.Controls.Add(pCodigo, 0, editando == null ? 1 : 0);

        // Nombre
        var pNombre = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(0, 8, 0, 0) };
        pNombre.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        pNombre.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        var lblNom = new Label { Text = "NOMBRE *", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.FromArgb(80, 80, 80) };
        pNombre.Controls.Add(lblNom, 0, 0);
        pNombre.Controls.Add(txtNombre, 1, 0);
        main.Controls.Add(pNombre, 0, editando == null ? 2 : 1);

        // Precio
        var pPrecio = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(0, 8, 0, 0) };
        pPrecio.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        pPrecio.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        var lblPre = new Label { Text = "PRECIO *", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.FromArgb(80, 80, 80) };
        pPrecio.Controls.Add(lblPre, 0, 0);
        pPrecio.Controls.Add(txtPrecio, 1, 0);
        main.Controls.Add(pPrecio, 0, editando == null ? 3 : 2);

        // Cantidad
        var pCant = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(0, 8, 0, 0) };
        pCant.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        pCant.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        var lblCant = new Label { Text = "CANTIDAD *", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.FromArgb(80, 80, 80) };
        pCant.Controls.Add(lblCant, 0, 0);
        pCant.Controls.Add(txtCantidad, 1, 0);
        main.Controls.Add(pCant, 0, editando == null ? 4 : 3);

        // Garantía (solo repuestos)
        var pGarantia = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, Padding = new Padding(0, 8, 0, 0) };
        pGarantia.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        pGarantia.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        pGarantia.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50));
        pGarantia.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        var lblGar = new Label { Text = "GARANTÍA", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.FromArgb(80, 80, 80) };
        var lblDias = new Label { Text = "Días:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 8) };
        pGarantia.Controls.Add(lblGar, 0, 0);
        pGarantia.Controls.Add(txtDiasGarantia, 1, 0);
        pGarantia.Controls.Add(lblDias, 2, 0);
        pGarantia.Controls.Add(dtpInicio, 3, 0);
        main.Controls.Add(pGarantia, 0, editando == null ? 5 : 4);

        // Botones
        var pBtns = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(0, 12, 0, 0) };
        pBtns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        pBtns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        var btnGuardar = new Button { Text = editando == null ? "➕  AGREGAR" : "💾  GUARDAR", Dock = DockStyle.Fill, BackColor = Color.FromArgb(0, 150, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Height = 40 };
        var btnCancel = new Button { Text = "Cancelar", Dock = DockStyle.Fill, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10), Height = 40 };
        btnGuardar.FlatAppearance.BorderSize = 0;
        btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;
        btnGuardar.Click += (s, e) => ValidarYGuardar();
        pBtns.Controls.Add(btnGuardar, 0, 0);
        pBtns.Controls.Add(btnCancel, 1, 0);
        main.Controls.Add(pBtns, 0, editando == null ? 7 : 6);

        Controls.Add(main);

        txtCodigo.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true; e.SuppressKeyPress = true;
                BuscarPorCodigoDirecto();
            }
        };

        if (editando != null)
        {
            CargarDatosEdicion(editando);
            ActualizarCamposPorTipo();
        }
        else
        {
            ActualizarCamposPorTipo();
        }

        txtCodigo.Focus();
    }

    void ActualizarCamposPorTipo()
    {
        int tipoIdx = _editando == null ? cmbTipo.SelectedIndex : (int)_tipo;
        bool esServicio = tipoIdx == 0;
        bool esRepuesto = tipoIdx == 1;
        bool esManual = tipoIdx == 2;

        txtCodigo.ReadOnly = false;
        txtNombre.ReadOnly = false;
        txtPrecio.ReadOnly = false;
        txtDiasGarantia.Enabled = esRepuesto;
        dtpInicio.Enabled = esRepuesto;
        btnBuscarCatalogo.Visible = esServicio || esRepuesto;
        btnBuscarCatalogo.Enabled = esServicio || esRepuesto;

        if (esServicio)
        {
            txtCodigo.PlaceholderText = "Código de servicio (ej: CAMB-ACEITE)";
            txtNombre.PlaceholderText = "Nombre del servicio";
            toolTip.SetToolTip(btnBuscarCatalogo, "Buscar en catálogo de servicios (F2)");
            this.AcceptButton = null;
        }
        else if (esRepuesto)
        {
            txtCodigo.PlaceholderText = "Código de inventario (ej: FILTRO-001)";
            txtNombre.PlaceholderText = "Nombre del repuesto";
            toolTip.SetToolTip(btnBuscarCatalogo, "Buscar en inventario (F2)");
            if (_editando == null) txtDiasGarantia.Value = 90;
        }
        else
        {
            txtCodigo.PlaceholderText = "Código interno (opcional)";
            txtNombre.PlaceholderText = "Descripción libre";
            btnBuscarCatalogo.Visible = false;
            txtDiasGarantia.Value = 0;
        }
    }

    void BuscarEnCatalogo()
    {
        int tipoIdx = _editando == null ? cmbTipo.SelectedIndex : (int)_tipo;
        bool esServicio = tipoIdx == 0;

        if (esServicio)
        {
            using var dlg = new BuscarServicioForm(_servicios);
            if (dlg.ShowDialog() == DialogResult.OK && dlg.Seleccionado != null)
            {
                var s = dlg.Seleccionado;
                txtCodigo.Text = s.Codigo;
                txtNombre.Text = s.Nombre;
                txtPrecio.Value = s.Precio;
                txtCantidad.Focus();
            }
        }
        else
        {
            using var dlg = new BuscarInventarioForm(_inventario);
            if (dlg.ShowDialog() == DialogResult.OK && dlg.Seleccionado != null)
            {
                var i = dlg.Seleccionado;
                txtCodigo.Text = i.Codigo;
                txtNombre.Text = i.Nombre;
                txtPrecio.Value = i.Precio;
                txtCantidad.Focus();
            }
        }
    }

    void BuscarPorCodigoDirecto()
    {
        int tipoIdx = _editando == null ? cmbTipo.SelectedIndex : (int)_tipo;
        var codigo = txtCodigo.Text.Trim().ToUpper();

        if (tipoIdx == 0) // Servicio
        {
            var s = _servicios.FirstOrDefault(x => x.Codigo.Equals(codigo, StringComparison.OrdinalIgnoreCase));
            if (s != null)
            {
                txtNombre.Text = s.Nombre;
                txtPrecio.Value = s.Precio;
                txtCantidad.Focus();
            }
        }
        else if (tipoIdx == 1) // Repuesto
        {
            var i = _inventario.FirstOrDefault(x => x.Codigo.Equals(codigo, StringComparison.OrdinalIgnoreCase));
            if (i != null)
            {
                txtNombre.Text = i.Nombre;
                txtPrecio.Value = i.Precio;
                txtCantidad.Focus();
            }
        }
    }

    void CargarDatosEdicion(ConceptoOrden c)
    {
        txtCodigo.Text = c.Codigo;
        txtNombre.Text = c.Nombre;
        txtPrecio.Value = c.Precio;
        txtCantidad.Value = c.Cantidad;
        txtDiasGarantia.Value = c.DiasGarantia;
        dtpInicio.Value = c.FechaInicio;
    }

    void ValidarYGuardar()
    {
        int tipoIdx = _editando == null ? cmbTipo.SelectedIndex : (int)_tipo;
        var tipo = tipoIdx == 0 ? TipoConcepto.Servicio : tipoIdx == 1 ? TipoConcepto.Repuesto : TipoConcepto.Manual;

        if (tipo != TipoConcepto.Manual && string.IsNullOrWhiteSpace(txtCodigo.Text))
        {
            MessageBox.Show("El código es obligatorio", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtCodigo.Focus(); return;
        }
        if (string.IsNullOrWhiteSpace(txtNombre.Text))
        {
            MessageBox.Show("El nombre es obligatorio", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtNombre.Focus(); return;
        }
        if (txtPrecio.Value < 0)
        {
            MessageBox.Show("El precio no puede ser negativo", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtPrecio.Focus(); return;
        }

        Concepto = new ConceptoOrden
        {
            Tipo = tipo,
            ServicioId = tipo == TipoConcepto.Servicio ? _servicios.FirstOrDefault(x => x.Codigo.Equals(txtCodigo.Text.Trim(), StringComparison.OrdinalIgnoreCase))?.Id : null,
            Codigo = txtCodigo.Text.Trim().ToUpper(),
            Nombre = txtNombre.Text.Trim(),
            Precio = txtPrecio.Value,
            Cantidad = (int)txtCantidad.Value,
            DiasGarantia = tipo == TipoConcepto.Repuesto ? (int)txtDiasGarantia.Value : 0,
            FechaInicio = dtpInicio.Value.Date
        };

        DialogResult = DialogResult.OK;
    }
}

public class BuscarServicioForm : Form
{
    public Servicio? Seleccionado { get; private set; }
    readonly List<Servicio> _servicios;
    TextBox txtFiltro = new() { PlaceholderText = "Buscar por código o nombre...", BorderStyle = BorderStyle.FixedSingle, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };
    DataGridView dgv = new() { Dock = DockStyle.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle, RowHeadersVisible = false };

    public BuscarServicioForm(List<Servicio> servicios)
    {
        _servicios = servicios;
        Text = "🔍 Buscar Servicio";
        ClientSize = new Size(650, 450);
        MinimumSize = new Size(650, 450);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.White;

        var main = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(10) };
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
        main.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

        var top = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 8) };
        top.Controls.Add(txtFiltro);
        txtFiltro.TextChanged += (s, e) => Buscar();

        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Codigo", HeaderText = "Código", DataPropertyName = "Codigo", FillWeight = 20 });
        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nombre", HeaderText = "Nombre", DataPropertyName = "Nombre", FillWeight = 40 });
        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Descripcion", HeaderText = "Descripción", DataPropertyName = "Descripcion", FillWeight = 25 });
        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Precio", HeaderText = "Precio", DataPropertyName = "Precio", FillWeight = 15, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight, Format = "C" } });

        dgv.DoubleClick += (s, e) => Seleccionar();

        var bottom = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0) };
        var btnSel = new Button { Text = "Seleccionar", Dock = DockStyle.Right, Width = 130, BackColor = Color.FromArgb(30, 60, 110), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Height = 36 };
        var btnCancel = new Button { Text = "Cancelar", Dock = DockStyle.Right, Width = 100, FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 0, 8, 0), Height = 36 };
        btnSel.FlatAppearance.BorderSize = 0;
        btnSel.Click += (s, e) => Seleccionar();
        btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;
        bottom.Controls.Add(btnSel); bottom.Controls.Add(btnCancel);

        main.Controls.Add(top, 0, 0);
        main.Controls.Add(dgv, 0, 1);
        main.Controls.Add(bottom, 0, 2);
        Controls.Add(main);

        txtFiltro.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) Seleccionar(); else if (e.KeyCode == Keys.Down) dgv.Focus(); };
        dgv.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) Seleccionar(); };

        Buscar();
    }

    void Buscar()
    {
        var f = txtFiltro.Text.Trim().ToLower();
        var list = string.IsNullOrWhiteSpace(f)
            ? _servicios
            : _servicios.Where(x => x.Codigo.ToLower().Contains(f) || x.Nombre.ToLower().Contains(f) || (x.Descripcion?.ToLower().Contains(f) ?? false)).ToList();
        dgv.DataSource = list.Select(s => new { s.Codigo, s.Nombre, s.Descripcion, Precio = s.Precio.ToString("C") }).ToList();
    }

    void Seleccionar()
    {
        if (dgv.CurrentRow == null) return;
        var cod = dgv.CurrentRow.Cells["Codigo"].Value?.ToString();
        if (cod == null) return;
        Seleccionado = _servicios.FirstOrDefault(x => x.Codigo == cod);
        DialogResult = DialogResult.OK;
    }
}

public class BuscarInventarioForm : Form
{
    public InventarioItem? Seleccionado { get; private set; }
    readonly List<InventarioItem> _inventario;
    TextBox txtFiltro = new() { PlaceholderText = "Buscar por código o nombre...", BorderStyle = BorderStyle.FixedSingle, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };
    DataGridView dgv = new() { Dock = DockStyle.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle, RowHeadersVisible = false };

    public BuscarInventarioForm(List<InventarioItem> inventario)
    {
        _inventario = inventario;
        Text = "🔍 Buscar Repuesto en Inventario";
        ClientSize = new Size(650, 450);
        MinimumSize = new Size(650, 450);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.White;

        var main = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(10) };
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
        main.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

        var top = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 8) };
        top.Controls.Add(txtFiltro);
        txtFiltro.TextChanged += (s, e) => Buscar();

        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Codigo", HeaderText = "Código", DataPropertyName = "Codigo", FillWeight = 20 });
        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nombre", HeaderText = "Nombre", DataPropertyName = "Nombre", FillWeight = 40 });
        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Existencia", HeaderText = "Stock", DataPropertyName = "Existencia", FillWeight = 12, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Precio", HeaderText = "Precio", DataPropertyName = "Precio", FillWeight = 15, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight, Format = "C" } });

        dgv.DoubleClick += (s, e) => Seleccionar();

        var bottom = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0) };
        var btnSel = new Button { Text = "Seleccionar", Dock = DockStyle.Right, Width = 130, BackColor = Color.FromArgb(30, 60, 110), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Height = 36 };
        var btnCancel = new Button { Text = "Cancelar", Dock = DockStyle.Right, Width = 100, FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 0, 8, 0), Height = 36 };
        btnSel.FlatAppearance.BorderSize = 0;
        btnSel.Click += (s, e) => Seleccionar();
        btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;
        bottom.Controls.Add(btnSel); bottom.Controls.Add(btnCancel);

        main.Controls.Add(top, 0, 0);
        main.Controls.Add(dgv, 0, 1);
        main.Controls.Add(bottom, 0, 2);
        Controls.Add(main);

        txtFiltro.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) Seleccionar(); else if (e.KeyCode == Keys.Down) dgv.Focus(); };
        dgv.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) Seleccionar(); };

        Buscar();
    }

    void Buscar()
    {
        var f = txtFiltro.Text.Trim().ToLower();
        var list = string.IsNullOrWhiteSpace(f)
            ? _inventario
            : _inventario.Where(x => x.Codigo.ToLower().Contains(f) || x.Nombre.ToLower().Contains(f)).ToList();
        dgv.DataSource = list.Select(i => new { i.Codigo, i.Nombre, i.Existencia, Precio = i.Precio.ToString("C") }).ToList();
    }

    void Seleccionar()
    {
        if (dgv.CurrentRow == null) return;
        var cod = dgv.CurrentRow.Cells["Codigo"].Value?.ToString();
        if (cod == null) return;
        Seleccionado = _inventario.FirstOrDefault(x => x.Codigo == cod);
        DialogResult = DialogResult.OK;
    }
}