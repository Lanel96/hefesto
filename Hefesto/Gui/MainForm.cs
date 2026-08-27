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

    public MainForm()
    {
        Text = "Hefesto - Sistema Taller Mecánico  |  DB: " + Db.DbPath;
        WindowState = FormWindowState.Maximized;
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9);

        // Header
        var header = new Panel { Height = 60, Dock = DockStyle.Top, BackColor = Color.FromArgb(30, 60, 110) };
        var lblLogo = new Label { Text = "⚙ HEFESTO", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(15, 15) };
        var lblSub = new Label { Text = "Gestión de Órdenes • Vehículos • Servicios • Garantías", Font = new Font("Segoe UI", 8), ForeColor = Color.FromArgb(180, 200, 255), AutoSize = true, Location = new Point(16, 38) };
        var lblVer = new Label { Text = $"v{Updater.CurrentVersion}", Font = new Font("Segoe UI", 7, FontStyle.Bold), ForeColor = Color.FromArgb(255, 230, 100), AutoSize = true, Location = new Point(155, 22) };
        var lblDb = new Label { Text = Db.DbPath, Font = new Font("Segoe UI", 7), ForeColor = Color.FromArgb(180, 200, 255), AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right };
        lblDb.Location = new Point(Width - 500, 22);
        var btnUpdate = new Button { Text = "🔄 Actualizar", Size = new Size(110, 26), Location = new Point(Width - 135, 32), Anchor = AnchorStyles.Top | AnchorStyles.Right, BackColor = Color.FromArgb(255, 193, 7), ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 7, FontStyle.Bold), Cursor = Cursors.Hand };
        btnUpdate.FlatAppearance.BorderSize = 0;
        btnUpdate.Click += async (s, e) => await CheckUpdatesAsync(false);
        header.Controls.AddRange(new Control[] { lblLogo, lblSub, lblVer, lblDb, btnUpdate });
        Controls.Add(header);

        tabs.ItemSize = new Size(120, 32);
        tabs.SizeMode = TabSizeMode.Fixed;
        tabs.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        tabs.TabPages.Add(MakeOrdenesTab());
        tabs.TabPages.Add(MakeVehiculosTab());
        tabs.TabPages.Add(MakeServiciosTab());
        tabs.TabPages.Add(MakeBitacoraTab());
        tabs.TabPages.Add(MakeConfigTab());

        var container = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8) };
        container.Controls.Add(tabs);
        Controls.Add(container);
        tabs.BringToFront();
        header.BringToFront();

        tabs.SelectedIndexChanged += (s, e) =>
        {
            if (tabs.SelectedIndex == 0) SafeLoad(LoadOrdenes, "Órdenes");
            if (tabs.SelectedIndex == 1) SafeLoad(LoadVehiculos, "Vehículos");
            if (tabs.SelectedIndex == 2) SafeLoad(LoadServicios, "Servicios");
            if (tabs.SelectedIndex == 3) SafeLoad(LoadBitacora, "Bitácora");
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
        var top = new Panel { Height = 50, Dock = DockStyle.Top };
        txtFiltroOrden.Size = new Size(280, 30); txtFiltroOrden.Location = new Point(5, 12);
        txtFiltroOrden.TextChanged += (s, e) => LoadOrdenes();
        var btnNueva = MakeBtn("➕ Nueva Orden", Color.FromArgb(0, 150, 80), new Point(300, 8), () => { using var f = new OrdenForm(); if (f.ShowDialog() == DialogResult.OK) LoadOrdenes(); });
        btnNueva.Size = new Size(150, 34);
        var btnVer = MakeBtn("👁 Ver Detalle", Color.FromArgb(30, 60, 110), new Point(460, 8), VerDetalle);
        var btnEstado = MakeBtn("🔄 Cambiar Estado", Color.FromArgb(200, 120, 0), new Point(590, 8), CambiarEstado);
        var btnDel = MakeBtn("🗑 Eliminar", Color.FromArgb(180, 40, 40), new Point(730, 8), EliminarOrden);
        top.Controls.AddRange(new Control[] { txtFiltroOrden, btnNueva, btnVer, btnEstado, btnDel });
        p.Controls.Add(dgvOrdenes); p.Controls.Add(top);
        dgvOrdenes.Dock = DockStyle.Fill;
        dgvOrdenes.DoubleClick += (s, e) => VerDetalle();
        return p;
    }

    TabPage MakeVehiculosTab()
    {
        var p = new TabPage("  🚗 VEHÍCULOS  ");
        var top = new Panel { Height = 50, Dock = DockStyle.Top };
        txtFiltroVeh.Size = new Size(280, 30); txtFiltroVeh.Location = new Point(5, 12);
        txtFiltroVeh.TextChanged += (s, e) => LoadVehiculos();
        var btnAdd = MakeBtn("➕ Nuevo / Editar", Color.FromArgb(0, 150, 80), new Point(300, 8), EditarVehiculo);
        var btnDel = MakeBtn("🗑 Eliminar", Color.FromArgb(180, 40, 40), new Point(450, 8), () => { if (dgvVeh.CurrentRow == null) return; var placa = dgvVeh.CurrentRow.Cells["Placa"].Value!.ToString()!; if (MessageBox.Show($"¿Eliminar {placa}?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes) { try { Repos.DeleteVehiculo(placa); LoadVehiculos(); } catch (Exception ex) { MessageBox.Show(ex.Message); } } });
        top.Controls.AddRange(new Control[] { txtFiltroVeh, btnAdd, btnDel });
        p.Controls.Add(dgvVeh); p.Controls.Add(top);
        dgvVeh.DoubleClick += (s, e) => EditarVehiculo();
        return p;
    }

    TabPage MakeServiciosTab()
    {
        var p = new TabPage("  🔧 CATÁLOGO SERVICIOS  ");
        var top = new Panel { Height = 50, Dock = DockStyle.Top };
        txtFiltroServ.Size = new Size(280, 30); txtFiltroServ.Location = new Point(5, 12);
        txtFiltroServ.TextChanged += (s, e) => LoadServicios();
        var btnAdd = MakeBtn("➕ Nuevo / Editar", Color.FromArgb(0, 150, 80), new Point(300, 8), EditarServicio);
        var btnDel = MakeBtn("🗑 Eliminar", Color.FromArgb(180, 40, 40), new Point(450, 8), () => { if (dgvServ.CurrentRow == null) return; var id = Convert.ToInt32(dgvServ.CurrentRow.Cells["Id"].Value); if (MessageBox.Show("¿Eliminar servicio?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes) { Repos.DeleteServicio(id); LoadServicios(); } });
        top.Controls.AddRange(new Control[] { txtFiltroServ, btnAdd, btnDel });
        p.Controls.Add(dgvServ); p.Controls.Add(top);
        dgvServ.DoubleClick += (s, e) => EditarServicio();
        return p;
    }

    TabPage MakeBitacoraTab()
    {
        var p = new TabPage("  📜 BITÁCORA / GARANTÍAS  ");
        var top = new Panel { Height = 50, Dock = DockStyle.Top };
        txtFiltroBit.Size = new Size(280, 30); txtFiltroBit.Location = new Point(5, 12);
        txtFiltroBit.TextChanged += (s, e) => LoadBitacora();
        var btnRefresh = MakeBtn("🔄 Actualizar", Color.FromArgb(30, 60, 110), new Point(300, 8), () => LoadBitacora());
        top.Controls.AddRange(new Control[] { txtFiltroBit, btnRefresh });
        var legend = new Label { Text = "Verde = EN GARANTÍA  |  Rojo = VENCIDA", ForeColor = Color.Gray, Font = new Font("Segoe UI", 8, FontStyle.Italic), AutoSize = true, Location = new Point(450, 17) };
        top.Controls.Add(legend);
        p.Controls.Add(dgvBit); p.Controls.Add(top);
        return p;
    }

    TabPage MakeConfigTab()
    {
        var p = new TabPage("  ⚙ CONFIGURACIÓN  ");
        var top = new Panel { Height = 80, Dock = DockStyle.Top, BackColor = Color.FromArgb(245, 245, 245) };
        lblRuta.Text = "Base actual: " + Db.DbPath;
        lblRuta.Location = new Point(10, 10); lblRuta.AutoSize = true; lblRuta.Font = new Font("Segoe UI", 8, FontStyle.Bold);
        var btnCambiar = MakeBtn("📁 Cambiar / Respaldar DB", Color.FromArgb(30, 60, 110), new Point(10, 35), CambiarDb);
        var lblUser = new Label { Text = "Usuarios del sistema:", Location = new Point(10, 75), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
        top.Controls.AddRange(new Control[] { lblRuta, btnCambiar });
        var panelUsers = new Panel { Dock = DockStyle.Fill };
        var topUsers = new Panel { Height = 45, Dock = DockStyle.Top };
        var btnAddU = MakeBtn("➕ Nuevo Usuario", Color.FromArgb(0, 150, 80), new Point(5, 6), AgregarUsuario);
        var btnDelU = MakeBtn("🗑 Eliminar", Color.FromArgb(180, 40, 40), new Point(160, 6), () => { if (dgvUsers.CurrentRow == null) return; var id = Convert.ToInt32(dgvUsers.CurrentRow.Cells["Id"].Value); if (MessageBox.Show("¿Eliminar usuario?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes) { Repos.DeleteUsuario(id); LoadUsuarios(); } });
        topUsers.Controls.AddRange(new Control[] { btnAddU, btnDelU });
        panelUsers.Controls.Add(dgvUsers); panelUsers.Controls.Add(topUsers);
        p.Controls.Add(panelUsers); p.Controls.Add(top); p.Controls.Add(lblUser);
        // fix layout
        lblUser.Dock = DockStyle.Top; top.Dock = DockStyle.Top; panelUsers.BringToFront();
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
        dgvServ.DataSource = Repos.GetServicios(txtFiltroServ.Text).Select(s => new { s.Id, s.Codigo, s.Nombre, s.Descripcion, Precio = s.Precio.ToString("C"), Duración = s.DuracionMin + " min" }).ToList();
        if (dgvServ.IsHandleCreated && dgvServ.Columns["Id"] != null)
        {
            try { dgvServ.Columns["Id"]!.Width = 40; } catch { }
        }
    }
    void LoadBitacora()
    {
        var reps = Repos.GetAllRepuestos(txtFiltroBit.Text);
        var data = reps.Select(r => new { r.OrdenId, r.Codigo, r.Nombre, Garantía = r.DiasGarantia + " días", Inicio = r.FechaInicio.ToString("dd/MM/yyyy"), Fin = r.FechaFin.ToString("dd/MM/yyyy"), Estado = r.EstadoGarantia }).ToList();
        dgvBit.DataSource = data;
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

// Formularios auxiliares pequeños
public class EstadoForm : Form
{
    public EstadoForm(int id, string actual)
    {
        Text = $"Cambiar Estado - Orden #{id}"; Size = new Size(350, 180); StartPosition = FormStartPosition.CenterParent; FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;
        var cmb = new ComboBox { Location = new Point(20, 30), Size = new Size(290, 30), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
        cmb.Items.AddRange(new[] { "Abierta", "En Proceso", "Finalizada", "Entregada", "Cancelada" }); cmb.SelectedItem = actual;
        var dtp = new DateTimePicker { Location = new Point(20, 70), Size = new Size(290, 30), Format = DateTimePickerFormat.Short, Value = DateTime.Now };
        var lbl = new Label { Text = "Fecha entrega (si aplica):", Location = new Point(20, 55), AutoSize = true, Font = new Font("Segoe UI", 7) };
        var btn = new Button { Text = "Guardar", Location = new Point(20, 105), Size = new Size(290, 32), BackColor = Color.FromArgb(30, 60, 110), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        btn.Click += (s, e) => { Repos.UpdateOrdenEstado(id, cmb.SelectedItem!.ToString()!, cmb.SelectedItem!.ToString() == "Entregada" || cmb.SelectedItem!.ToString() == "Finalizada" ? dtp.Value : null); DialogResult = DialogResult.OK; };
        Controls.AddRange(new Control[] { cmb, lbl, dtp, btn });
    }
}
