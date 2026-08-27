using Hefesto.Core;

namespace Hefesto.Gui;

public class MainForm : Form
{
    TabControl tabs = new() { Dock = DockStyle.Fill };
    // Ordenes
    DataGridView dgvOrdenes = new() { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
    TextBox txtFiltroOrden = new() { PlaceholderText = "Buscar por placa o estado..." };
    // Vehiculos
    DataGridView dgvVeh = new() { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
    TextBox txtFiltroVeh = new() { PlaceholderText = "Buscar placa, marca, cliente..." };
    // Servicios
    DataGridView dgvServ = new() { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
    TextBox txtFiltroServ = new() { PlaceholderText = "Buscar código o nombre..." };
    // Bitacora
    DataGridView dgvBit = new() { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
    TextBox txtFiltroBit = new() { PlaceholderText = "Buscar repuesto por código/nombre..." };
    // Config
    DataGridView dgvUsers = new() { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
    Label lblRuta = new();

    void ConfigureGrid(DataGridView dgv)
    {
        dgv.EnableHeadersVisualStyles = false;
        dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 60, 110);
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8, FontStyle.Bold);
        dgv.ColumnHeadersHeight = 32;
        dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        dgv.RowHeadersVisible = false;
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgv.ReadOnly = true;
        dgv.AllowUserToAddRows = false;
        dgv.AllowUserToResizeRows = false;
        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgv.BackgroundColor = Color.White;
        dgv.BorderStyle = BorderStyle.FixedSingle;
        dgv.GridColor = Color.FromArgb(220, 220, 220);
        dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);
        dgv.DefaultCellStyle.Font = new Font("Segoe UI", 8.5F);
        dgv.DefaultCellStyle.Padding = new Padding(2);
    }

    public MainForm()
    {
        Text = "Hefesto - Sistema Taller Mecánico  |  DB: " + Db.DbPath;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1200, 700);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9);

        // Header - sin leyenda de ruta (petición usuario) y TableLayout para no tapar
        var header = new Panel { Height = 78, Dock = DockStyle.Fill, BackColor = Color.FromArgb(30, 60, 110) };
        var lblLogo = new Label { Text = "⚙ HEFESTO", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(15, 12) };
        var lblSub = new Label { Text = "Gestión de Órdenes • Vehículos • Servicios • Garantías", Font = new Font("Segoe UI", 8), ForeColor = Color.FromArgb(180, 200, 255), AutoSize = true, Location = new Point(16, 36) };
        var lblVer = new Label { Text = $"v{Updater.CurrentVersion}", Font = new Font("Segoe UI", 7, FontStyle.Bold), ForeColor = Color.FromArgb(255, 230, 100), AutoSize = true, Location = new Point(155, 18) };
        var btnUpdate = new Button { Text = "🔄 Actualizar", Size = new Size(115, 28), BackColor = Color.FromArgb(255, 193, 7), ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8, FontStyle.Bold), Cursor = Cursors.Hand, Anchor = AnchorStyles.Top | AnchorStyles.Right };
        btnUpdate.FlatAppearance.BorderSize = 0;
        btnUpdate.Click += async (s, e) => await CheckUpdatesAsync(false);
        header.Controls.AddRange(new Control[] { lblLogo, lblSub, lblVer, btnUpdate });
        header.Resize += (s, e) =>
        {
            btnUpdate.Location = new Point(header.Width - 130, 24);
        };

        tabs.ItemSize = new Size(185, 38);
        tabs.SizeMode = TabSizeMode.Fixed;
        tabs.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        tabs.DrawMode = TabDrawMode.Normal;
        tabs.Padding = new Point(12, 4);
        tabs.TabPages.Add(MakeOrdenesTab());
        tabs.TabPages.Add(MakeVehiculosTab());
        tabs.TabPages.Add(MakeServiciosTab());
        tabs.TabPages.Add(MakeBitacoraTab());
        tabs.TabPages.Add(MakeUsersTab());
        tabs.TabPages.Add(MakeConfigTab());

        // Estilo profesional senior para todas las grillas
        ConfigureGrid(dgvOrdenes); ConfigureGrid(dgvVeh); ConfigureGrid(dgvServ); ConfigureGrid(dgvBit); ConfigureGrid(dgvUsers);

        var container = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8, 8, 8, 8) };
        tabs.Dock = DockStyle.Fill;
        container.Controls.Add(tabs);

        // Layout sin solape: TableLayout 78px header + resto
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, Padding = new Padding(0), Margin = new Padding(0) };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(container, 0, 1);
        Controls.Add(layout);

        tabs.SelectedIndexChanged += (s, e) =>
        {
            if (tabs.SelectedIndex == 0) SafeLoad(LoadOrdenes, "Órdenes");
            if (tabs.SelectedIndex == 1) SafeLoad(LoadVehiculos, "Vehículos");
            if (tabs.SelectedIndex == 2) SafeLoad(LoadServicios, "Servicios");
            if (tabs.SelectedIndex == 3) SafeLoad(LoadBitacora, "Bitácora");
            if (tabs.SelectedIndex == 4) SafeLoad(LoadUsuarios, "Usuarios");
        };
        Shown += async (s, e) => await CheckUpdatesAsync(true);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        SafeLoad(LoadOrdenes, "Órdenes");
        SafeLoad(LoadVehiculos, "Vehículos");
        SafeLoad(LoadServicios, "Servicios");
        SafeLoad(LoadBitacora, "Bitácora");
        SafeLoad(LoadUsuarios, "Usuarios");
    }

    TabPage MakeOrdenesTab()
    {
        var p = new TabPage("  📋 ÓRDENES  ");
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(5), BackColor = Color.FromArgb(248,248,248) };
        txtFiltroOrden.Size = new Size(260, 30); txtFiltroOrden.Margin = new Padding(0, 5, 8, 5);
        txtFiltroOrden.TextChanged += (s, e) => LoadOrdenes();
        var btnNueva = MakeBtn("➕ Nueva Orden", Color.FromArgb(0, 150, 80), () => { using var f = new OrdenForm(); if (f.ShowDialog() == DialogResult.OK) LoadOrdenes(); });
        btnNueva.Size = new Size(145, 32); btnNueva.Margin = new Padding(2, 4, 2, 4);
        var btnVer = MakeBtn("👁 Ver Detalle", Color.FromArgb(30, 60, 110), VerDetalle); btnVer.Margin = new Padding(2, 4, 2, 4);
        var btnEstado = MakeBtn("🔄 Cambiar Estado", Color.FromArgb(200, 120, 0), CambiarEstado); btnEstado.Margin = new Padding(2, 4, 2, 4);
        var btnDel = MakeBtn("🗑 Eliminar", Color.FromArgb(180, 40, 40), EliminarOrden); btnDel.Margin = new Padding(2, 4, 2, 4);
        top.Controls.AddRange(new Control[] { txtFiltroOrden, btnNueva, btnVer, btnEstado, btnDel });
        p.Controls.Add(dgvOrdenes); p.Controls.Add(top);
        dgvOrdenes.Dock = DockStyle.Fill;
        dgvOrdenes.DoubleClick += (s, e) => VerDetalle();
        return p;
    }

    TabPage MakeVehiculosTab()
    {
        var p = new TabPage("  🚗 VEHÍCULOS  ");
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(5), BackColor = Color.FromArgb(248,248,248) };
        txtFiltroVeh.Size = new Size(260, 30); txtFiltroVeh.Margin = new Padding(0, 5, 8, 5);
        txtFiltroVeh.TextChanged += (s, e) => LoadVehiculos();
        var btnAdd = MakeBtn("➕ Nuevo / Editar", Color.FromArgb(0, 150, 80), EditarVehiculo); btnAdd.Margin = new Padding(2, 4, 2, 4);
        var btnDel = MakeBtn("🗑 Eliminar", Color.FromArgb(180, 40, 40), () => { if (dgvVeh.CurrentRow == null) return; var placa = dgvVeh.CurrentRow.Cells["Placa"].Value!.ToString()!; if (MessageBox.Show($"¿Eliminar {placa}?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes) { try { Repos.DeleteVehiculo(placa); LoadVehiculos(); } catch (Exception ex) { MessageBox.Show(ex.Message); } } }); btnDel.Margin = new Padding(2, 4, 2, 4);
        top.Controls.AddRange(new Control[] { txtFiltroVeh, btnAdd, btnDel });
        p.Controls.Add(dgvVeh); p.Controls.Add(top);
        dgvVeh.DoubleClick += (s, e) => EditarVehiculo();
        return p;
    }

    TabPage MakeServiciosTab()
    {
        var p = new TabPage("  🔧 CATÁLOGO SERVICIOS  ");
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(5), BackColor = Color.FromArgb(248,248,248) };
        txtFiltroServ.Size = new Size(260, 32); txtFiltroServ.Margin = new Padding(0, 6, 8, 6);
        txtFiltroServ.TextChanged += (s, e) => LoadServicios();
        var btnAdd = MakeBtn("➕ Nuevo Servicio", Color.FromArgb(0, 150, 80), EditarServicio); btnAdd.Size = new Size(155, 34); btnAdd.Margin = new Padding(2, 4, 2, 4);
        var btnDel = MakeBtn("🗑 Eliminar", Color.FromArgb(180, 40, 40), () => { if (dgvServ.CurrentRow == null) return; var id = Convert.ToInt32(dgvServ.CurrentRow.Cells["Id"].Value); if (MessageBox.Show("¿Eliminar servicio?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes) { Repos.DeleteServicio(id); LoadServicios(); } }); btnDel.Size = new Size(125, 34); btnDel.Margin = new Padding(2, 4, 2, 4);
        var lblInfo = new Label { Text = "Define tus precios aquí", ForeColor = Color.Gray, Font = new Font("Segoe UI", 7, FontStyle.Italic), AutoSize = true, Margin = new Padding(10, 12, 0, 0) };
        top.Controls.AddRange(new Control[] { txtFiltroServ, btnAdd, btnDel, lblInfo });
        p.Controls.Add(dgvServ); p.Controls.Add(top);
        dgvServ.DoubleClick += (s, e) => EditarServicio();
        return p;
    }

    TabPage MakeBitacoraTab()
    {
        var p = new TabPage("  📜 BITÁCORA / GARANTÍAS  ");
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(5), BackColor = Color.FromArgb(248,248,248) };
        txtFiltroBit.Size = new Size(260, 32); txtFiltroBit.Margin = new Padding(0, 6, 8, 6);
        txtFiltroBit.TextChanged += (s, e) => LoadBitacora();
        var btnRefresh = MakeBtn("🔄 Actualizar", Color.FromArgb(30, 60, 110), () => LoadBitacora()); btnRefresh.Size = new Size(145, 34); btnRefresh.Margin = new Padding(2, 4, 2, 4);
        var legend = new Label { Text = "Verde = EN GARANTÍA  |  Rojo = VENCIDA", ForeColor = Color.Gray, Font = new Font("Segoe UI", 8, FontStyle.Italic), AutoSize = true, Margin = new Padding(10, 12, 0, 0) };
        top.Controls.AddRange(new Control[] { txtFiltroBit, btnRefresh, legend });
        p.Controls.Add(dgvBit); p.Controls.Add(top);
        return p;
    }

    TabPage MakeUsersTab()
    {
        var p = new TabPage("  👥 USUARIOS  ");
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(8), BackColor = Color.FromArgb(248,248,248) };
        var lblInfo = new Label { Text = "Módulo personalizado: crea usuarios del taller (Admin/Mecánico/Cajero)", ForeColor = Color.Gray, Font = new Font("Segoe UI", 8, FontStyle.Italic), AutoSize = true, Margin = new Padding(0, 8, 12, 0) };
        var btnAddU = MakeBtn("➕ Nuevo Usuario", Color.FromArgb(0, 150, 80), AgregarUsuario); btnAddU.Margin = new Padding(4);
        var btnDelU = MakeBtn("🗑 Eliminar", Color.FromArgb(180, 40, 40), () => { if (dgvUsers.CurrentRow == null) return; var id = Convert.ToInt32(dgvUsers.CurrentRow.Cells["Id"].Value); if (MessageBox.Show("¿Eliminar usuario?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes) { Repos.DeleteUsuario(id); LoadUsuarios(); } }); btnDelU.Margin = new Padding(4);
        var btnEditU = MakeBtn("✏ Editar", Color.FromArgb(30, 60, 110), () => { if (dgvUsers.CurrentRow == null) return; var id = Convert.ToInt32(dgvUsers.CurrentRow.Cells["Id"].Value); var u = Repos.GetUsuarios().FirstOrDefault(x=>x.Id==id); if(u!=null){ using var f=new UsuarioForm(u.Username, u.Rol); if(f.ShowDialog()==DialogResult.OK) LoadUsuarios(); } }); btnEditU.Margin = new Padding(4);
        top.Controls.AddRange(new Control[] { btnAddU, btnDelU, btnEditU, lblInfo });
        p.Controls.Add(dgvUsers); p.Controls.Add(top);
        dgvUsers.Dock = DockStyle.Fill;
        return p;
    }

    TabPage MakeConfigTab()
    {
        var p = new TabPage("  ⚙ CONFIGURACIÓN  ");
        var top = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = Color.FromArgb(245, 245, 245), Padding = new Padding(10) };
        lblRuta.Text = "Base actual: " + Db.DbPath;
        lblRuta.AutoSize = false; lblRuta.Size = new Size(700, 20); lblRuta.Location = new Point(10, 10); lblRuta.Font = new Font("Segoe UI", 8, FontStyle.Bold);
        var btnCambiar = MakeBtn("📁 Cambiar / Respaldar DB", Color.FromArgb(30, 60, 110), new Point(10, 35), CambiarDb);
        var lblVer2 = new Label { Text = $"Versión: v{Updater.CurrentVersion}  |  Repo: {Updater.Repo}  |  SQLite embebido", AutoSize = true, Location = new Point(10, 70), Font = new Font("Segoe UI", 7), ForeColor = Color.Gray };
        top.Controls.AddRange(new Control[] { lblRuta, btnCambiar, lblVer2 });
        var lblInfo2 = new Label { Text = "La base se guarda junto al exe (hefesto.db) y es portable. Usa 'Cambiar' para respaldar.", Dock = DockStyle.Top, Height = 30, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 5, 0, 0), ForeColor = Color.Gray, Font = new Font("Segoe UI", 8, FontStyle.Italic) };
        p.Controls.Add(lblInfo2); p.Controls.Add(top);
        return p;
    }

    void SafeLoad(Action act, string name)
    {
        try { act(); }
        catch (Exception ex)
        {
            MessageBox.Show($"Error cargando {name}:\n{ex.Message}\n\n{ex.StackTrace}", "Hefesto - Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            try { File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "hefesto.log"), $"[{DateTime.Now}] SafeLoad {name} FAIL: {ex}{Environment.NewLine}"); } catch { }
        }
    }

    Button MakeBtn(string text, Color bg, Point loc, Action act)
    {
        var b = new Button { Text = text, Location = loc, Size = new Size(120, 32), BackColor = bg, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8, FontStyle.Bold) };
        b.FlatAppearance.BorderSize = 0; b.Click += (s, e) => { try { act(); } catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}\n\n{ex.StackTrace}"); } }; return b;
    }
    Button MakeBtn(string text, Color bg, Action act)
    {
        var b = new Button { Text = text, Size = new Size(120, 32), BackColor = bg, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8, FontStyle.Bold) };
        b.FlatAppearance.BorderSize = 0; b.Click += (s, e) => { try { act(); } catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}\n\n{ex.StackTrace}"); } }; return b;
    }

    void LoadOrdenes()
    {
        var data = Repos.GetOrdenes(txtFiltroOrden.Text);
        dgvOrdenes.DataSource = data.Select(o => new { o.Id, o.Placa, Fecha_Ingreso = o.FechaIngreso.ToString("dd/MM/yyyy HH:mm"), Fecha_Entrega = o.FechaEntrega?.ToString("dd/MM/yyyy") ?? "-", o.Estado, Total = o.Total.ToString("C"), o.Observaciones }).ToList();
        // Ajuste de ancho diferido y seguro (handle debe existir)
        if (dgvOrdenes.IsHandleCreated && dgvOrdenes.Columns["Id"] != null)
        {
            try { dgvOrdenes.Columns["Id"]!.Width = 50; } catch { }
        }
    }
    void LoadVehiculos()
    {
        dgvVeh.DataSource = Repos.GetVehiculos(txtFiltroVeh.Text).Select(v => new { v.Placa, v.Marca, v.Modelo, Año = v.Anio?.ToString() ?? "-", v.Cliente, v.Telefono }).ToList();
    }
    void LoadServicios()
    {
        var data = Repos.GetServicios(txtFiltroServ.Text).Select(s => new { s.Id, s.Codigo, s.Nombre, s.Descripcion, Precio = s.Precio.ToString("C"), Duración = s.DuracionMin + " min" }).ToList();
        dgvServ.DataSource = data;
        if (dgvServ.IsHandleCreated)
        {
            try
            {
                if (dgvServ.Columns["Id"] != null) { dgvServ.Columns["Id"]!.FillWeight = 8; dgvServ.Columns["Id"]!.MinimumWidth = 50; }
                if (dgvServ.Columns["Codigo"] != null) { dgvServ.Columns["Codigo"]!.FillWeight = 15; dgvServ.Columns["Codigo"]!.MinimumWidth = 85; }
                if (dgvServ.Columns["Nombre"] != null) { dgvServ.Columns["Nombre"]!.FillWeight = 25; dgvServ.Columns["Nombre"]!.MinimumWidth = 120; }
                if (dgvServ.Columns["Descripcion"] != null) dgvServ.Columns["Descripcion"]!.FillWeight = 27;
                if (dgvServ.Columns["Precio"] != null) { dgvServ.Columns["Precio"]!.FillWeight = 12; dgvServ.Columns["Precio"]!.MinimumWidth = 80; }
                if (dgvServ.Columns["Duración"] != null) { dgvServ.Columns["Duración"]!.FillWeight = 13; dgvServ.Columns["Duración"]!.MinimumWidth = 85; dgvServ.Columns["Duración"]!.HeaderText = "Duración"; }
            } catch { }
        }
        // Empty state profesional
        if (data.Count == 0 && string.IsNullOrWhiteSpace(txtFiltroServ.Text))
        {
            dgvServ.BackgroundColor = Color.FromArgb(255, 250, 230);
        }
        else dgvServ.BackgroundColor = Color.White;
    }
    void LoadBitacora()
    {
        var reps = Repos.GetAllRepuestos(txtFiltroBit.Text);
        var data = reps.Select(r => new { r.OrdenId, r.Codigo, r.Nombre, Garantía = r.DiasGarantia + " días", Inicio = r.FechaInicio.ToString("dd/MM/yyyy"), Fin = r.FechaFin.ToString("dd/MM/yyyy"), Estado = r.EstadoGarantia }).ToList();
        dgvBit.DataSource = data;
        if (dgvBit.IsHandleCreated)
        {
            try
            {
                if (dgvBit.Columns["OrdenId"] != null) { dgvBit.Columns["OrdenId"]!.FillWeight = 10; dgvBit.Columns["OrdenId"]!.MinimumWidth = 65; dgvBit.Columns["OrdenId"]!.HeaderText = "Orden"; }
                if (dgvBit.Columns["Codigo"] != null) { dgvBit.Columns["Codigo"]!.FillWeight = 14; dgvBit.Columns["Codigo"]!.MinimumWidth = 80; }
                if (dgvBit.Columns["Nombre"] != null) { dgvBit.Columns["Nombre"]!.FillWeight = 20; dgvBit.Columns["Nombre"]!.MinimumWidth = 110; }
                if (dgvBit.Columns["Garantía"] != null) { dgvBit.Columns["Garantía"]!.FillWeight = 13; dgvBit.Columns["Garantía"]!.MinimumWidth = 90; dgvBit.Columns["Garantía"]!.HeaderText = "Garantía"; }
                if (dgvBit.Columns["Inicio"] != null) { dgvBit.Columns["Inicio"]!.FillWeight = 13; dgvBit.Columns["Inicio"]!.MinimumWidth = 85; }
                if (dgvBit.Columns["Fin"] != null) { dgvBit.Columns["Fin"]!.FillWeight = 13; dgvBit.Columns["Fin"]!.MinimumWidth = 85; }
                if (dgvBit.Columns["Estado"] != null) { dgvBit.Columns["Estado"]!.FillWeight = 17; dgvBit.Columns["Estado"]!.MinimumWidth = 130; }
            } catch { }
        }
        foreach (DataGridViewRow row in dgvBit.Rows)
        {
            var estado = row.Cells["Estado"].Value?.ToString() ?? "";
            row.DefaultCellStyle.BackColor = estado.StartsWith("EN GARANTÍA") ? Color.FromArgb(220, 255, 220) : Color.FromArgb(255, 220, 220);
            row.DefaultCellStyle.ForeColor = estado.StartsWith("EN GARANTÍA") ? Color.FromArgb(0, 100, 0) : Color.FromArgb(150, 0, 0);
        }
    }
    void LoadUsuarios() { dgvUsers.DataSource = Repos.GetUsuarios().Select(u => new { u.Id, u.Username, u.Rol }).ToList(); }

    void VerDetalle()
    {
        if (dgvOrdenes.CurrentRow == null) return;
        int id = Convert.ToInt32(dgvOrdenes.CurrentRow.Cells["Id"].Value);
        using var f = new DetalleOrdenForm(id);
        f.ShowDialog();
        LoadOrdenes(); LoadBitacora();
    }
    void CambiarEstado()
    {
        if (dgvOrdenes.CurrentRow == null) return;
        int id = Convert.ToInt32(dgvOrdenes.CurrentRow.Cells["Id"].Value);
        string cur = dgvOrdenes.CurrentRow.Cells["Estado"].Value!.ToString()!;
        using var f = new EstadoForm(id, cur);
        if (f.ShowDialog() == DialogResult.OK) LoadOrdenes();
    }
    void EliminarOrden()
    {
        if (dgvOrdenes.CurrentRow == null) return;
        int id = Convert.ToInt32(dgvOrdenes.CurrentRow.Cells["Id"].Value);
        if (MessageBox.Show($"¿Eliminar orden #{id}? Se borrarán servicios y garantías.", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
        { Repos.DeleteOrden(id); LoadOrdenes(); LoadBitacora(); }
    }
    void EditarVehiculo()
    {
        string? placa = dgvVeh.CurrentRow?.Cells["Placa"].Value?.ToString();
        Vehiculo? v = null;
        if (placa != null) v = Repos.GetVehiculos(placa).FirstOrDefault(x => x.Placa == placa);
        using var f = new VehiculoForm(v);
        if (f.ShowDialog() == DialogResult.OK) LoadVehiculos();
    }
    void EditarServicio()
    {
        Servicio? s = null;
        if (dgvServ.CurrentRow != null)
        {
            int id = Convert.ToInt32(dgvServ.CurrentRow.Cells["Id"].Value);
            s = Repos.GetServicios().FirstOrDefault(x => x.Id == id);
        }
        using var f = new ServicioForm(s);
        if (f.ShowDialog() == DialogResult.OK) LoadServicios();
    }
    void AgregarUsuario()
    {
        using var f = new UsuarioForm(); if (f.ShowDialog() == DialogResult.OK) LoadUsuarios();
    }
    async Task CheckUpdatesAsync(bool silent)
    {
        try
        {
            var (hasUpdate, latest, url, notes) = await Updater.CheckAsync();
            if (hasUpdate && !string.IsNullOrEmpty(url))
            {
                var res = MessageBox.Show($"Nueva versión disponible: v{latest}\nActual: v{Updater.CurrentVersion}\n\nNotas:\n{notes}\n\n¿Descargar e instalar ahora?", "Hefesto - Actualización", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (res == DialogResult.Yes)
                {
                    var prog = new Form { Text = "Descargando actualización...", Size = new Size(400, 100), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, ControlBox = false };
                    var bar = new ProgressBar { Style = ProgressBarStyle.Continuous, Minimum = 0, Maximum = 100, Value = 0, Dock = DockStyle.Fill, Height = 30 };
                    prog.Controls.Add(bar);
                    prog.Shown += async (s, e) =>
                    {
                        try { await Updater.DownloadAndInstallAsync(url, latest, new Progress<int>(v => bar.Value = v)); }
                        catch (Exception ex) { MessageBox.Show("Error descargando: " + ex.Message); prog.Close(); }
                    };
                    prog.ShowDialog();
                }
            }
            else if (!silent)
            {
                if (Updater.Repo.Contains("TU_USUARIO")) MessageBox.Show($"Actualizador no configurado.\nEdita Hefesto/Gui/Updater.cs y pon tu repo GitHub (ej. tu-usuario/hefesto).\n\nVersión actual: v{Updater.CurrentVersion}", "Actualización", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else MessageBox.Show($"Estás al día. Versión actual: v{Updater.CurrentVersion}\nÚltima en GitHub: v{latest}", "Actualización", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex) { if (!silent) MessageBox.Show("Error verificando actualización:\n" + ex.Message); }
    }

    void CambiarDb()
    {
        using var sfd = new SaveFileDialog { Filter = "Base Hefesto (*.db)|*.db", FileName = Path.GetFileName(Db.DbPath), Title = "Nueva ubicación (se copiará la base actual)" };
        if (sfd.ShowDialog() != DialogResult.OK) return;
        try
        {
            File.Copy(Db.DbPath, sfd.FileName, true);
            var cfg = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string toStore = sfd.FileName;
            try { var abs = Path.GetFullPath(sfd.FileName); var baseFull = Path.GetFullPath(baseDir); if (abs.StartsWith(baseFull, StringComparison.OrdinalIgnoreCase)) toStore = Path.GetRelativePath(baseFull, abs); } catch { }
            File.WriteAllText(cfg, System.Text.Json.JsonSerializer.Serialize(new { DbPath = toStore }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
            MessageBox.Show($"Base copiada a:\n{sfd.FileName}\n\nReinicie la aplicación para usar la nueva ubicación.", "Configuración", MessageBoxButtons.OK, MessageBoxIcon.Information);
            lblRuta.Text = "Base actual: " + Db.DbPath + "  -> Nueva: " + sfd.FileName + " (reinicie)";
        }
        catch (Exception ex) { MessageBox.Show(ex.Message); }
    }
}

// Formularios auxiliares pequeños - tamaños aumentados para no cortar en DPI 125%
public class EstadoForm : Form
{
    public EstadoForm(int id, string actual)
    {
        Text = $"Cambiar Estado - Orden #{id}"; ClientSize = new Size(360, 210); MinimumSize = new Size(380, 240); StartPosition = FormStartPosition.CenterParent; FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false; AutoScaleMode = AutoScaleMode.Dpi; BackColor = Color.White;
        var cmb = new ComboBox { Location = new Point(20, 30), Size = new Size(320, 30), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
        cmb.Items.AddRange(new[] { "Abierta", "En Proceso", "Finalizada", "Entregada", "Cancelada" }); cmb.SelectedItem = actual;
        var dtp = new DateTimePicker { Location = new Point(20, 85), Size = new Size(320, 30), Format = DateTimePickerFormat.Short, Value = DateTime.Now };
        var lbl = new Label { Text = "Fecha entrega (si aplica):", Location = new Point(20, 65), AutoSize = true, Font = new Font("Segoe UI", 7) };
        var btn = new Button { Text = "💾 Guardar", Location = new Point(20, 130), Size = new Size(320, 36), BackColor = Color.FromArgb(30, 60, 110), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
        btn.FlatAppearance.BorderSize = 0;
        btn.Click += (s, e) => { Repos.UpdateOrdenEstado(id, cmb.SelectedItem!.ToString()!, cmb.SelectedItem!.ToString() == "Entregada" || cmb.SelectedItem!.ToString() == "Finalizada" ? dtp.Value : null); DialogResult = DialogResult.OK; };
        Controls.AddRange(new Control[] { cmb, lbl, dtp, btn });
    }
}
