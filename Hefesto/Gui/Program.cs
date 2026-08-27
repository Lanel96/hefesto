using Hefesto.Core;
using System.Text.Json;

namespace Hefesto.Gui;

static class Program
{
    static string LogPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "hefesto.log");
    static void Log(string msg)
    {
        try { File.AppendAllText(LogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {msg}{Environment.NewLine}"); } catch { }
    }

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (s, e) =>
        {
            Log($"ThreadException: {e.Exception}");
            MessageBox.Show($"Error no controlado:\n{e.Exception.Message}\n\nDetalles en hefesto.log\nRuta: {e.Exception.StackTrace}", "Hefesto - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        };
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            Log($"UnhandledException: {e.ExceptionObject}");
            MessageBox.Show($"Error fatal:\n{e.ExceptionObject}", "Hefesto - Error Fatal", MessageBoxButtons.OK, MessageBoxIcon.Error);
        };

        Log($"=== Inicio Hefesto v{Updater.CurrentVersion} ===");
        Log($"BaseDirectory: {AppDomain.CurrentDomain.BaseDirectory}");

        try { SQLitePCL.Batteries_V2.Init(); Log("SQLite batteries init OK"); } catch (Exception ex) { Log($"SQLite init warn: {ex.Message}"); }

        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string configPath = Path.Combine(baseDir, "appsettings.json");
        string dbPathRaw = "";
        string repoRaw = "";
        try
        {
            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("DbPath", out var p)) dbPathRaw = p.GetString() ?? "";
                if (doc.RootElement.TryGetProperty("GitHubRepo", out var r)) repoRaw = r.GetString() ?? "";
                if (!string.IsNullOrWhiteSpace(repoRaw) && !repoRaw.Contains("TU_USUARIO")) Updater.Repo = repoRaw.Trim();
                Log($"Config leido DbPath='{dbPathRaw}' Repo='{Updater.Repo}'");
            }
            else Log($"No existe config {configPath}, usando defaults");
        } catch (Exception ex) { Log($"Error leyendo config: {ex.Message}"); }

        string ResolveDbPath(string? configured, string baseDirectory)
        {
            if (string.IsNullOrWhiteSpace(configured)) return Path.Combine(baseDirectory, "hefesto.db");
            var trimmed = configured.Trim();
            if (!Path.IsPathRooted(trimmed)) return Path.GetFullPath(Path.Combine(baseDirectory, trimmed));
            var abs = Path.GetFullPath(trimmed);
            var baseFull = Path.GetFullPath(baseDirectory);
            if (abs.StartsWith(baseFull, StringComparison.OrdinalIgnoreCase)) return abs;
            if (!File.Exists(abs) && (abs.Contains("OracleReportGenerator", StringComparison.OrdinalIgnoreCase) || abs.Contains("\\bin\\", StringComparison.OrdinalIgnoreCase) || abs.Contains("publish-hefesto", StringComparison.OrdinalIgnoreCase)))
                return Path.Combine(baseDirectory, "hefesto.db");
            var dir = Path.GetDirectoryName(abs);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir) && !File.Exists(abs))
                return Path.Combine(baseDirectory, "hefesto.db");
            return abs;
        }

        string GetWritablePath(string preferred)
        {
            try
            {
                var dir = Path.GetDirectoryName(preferred);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
                if (!string.IsNullOrEmpty(dir))
                {
                    var test = Path.Combine(dir, ".writetest");
                    File.WriteAllText(test, "ok");
                    File.Delete(test);
                }
                return preferred;
            }
            catch (Exception ex)
            {
                Log($"GetWritablePath fallback por {ex.Message}");
                var fallbackDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Hefesto");
                Directory.CreateDirectory(fallbackDir);
                return Path.Combine(fallbackDir, "hefesto.db");
            }
        }

        var defaultPath = ResolveDbPath(dbPathRaw, baseDir);
        if (!string.IsNullOrWhiteSpace(dbPathRaw) && !Path.IsPathRooted(dbPathRaw) == false)
        {
            var resolvedCheck = ResolveDbPath(dbPathRaw, baseDir);
            if (resolvedCheck != Path.GetFullPath(dbPathRaw.Trim()) && !File.Exists(Path.GetFullPath(dbPathRaw.Trim())))
            {
                SaveConfig(configPath, resolvedCheck, baseDir);
                defaultPath = resolvedCheck;
            }
        }
        defaultPath = GetWritablePath(defaultPath);
        Log($"DbPath resuelto: {defaultPath}");
        Db.Configure(defaultPath);

        if (!File.Exists(Db.DbPath))
        {
            Log($"DB no existe en {Db.DbPath}, preguntando usuario");
            var res = MessageBox.Show(
                $"No se encontró la base de datos en:\n{Db.DbPath}\n\n¿Desea crear una nueva base en esa ubicación?\n\nSí = Crear aquí (SQLite embebido, no requiere instalación)\nNo = Seleccionar otra ubicación\nCancelar = Salir",
                "Hefesto - Primera ejecución", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (res == DialogResult.Cancel) { Log("Usuario canceló creación DB"); return; }
            if (res == DialogResult.No)
            {
                using var sfd = new SaveFileDialog { Filter = "Base Hefesto (*.db)|*.db", FileName = "hefesto.db", Title = "Seleccione ubicación de la base de datos" };
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    var chosen = GetWritablePath(sfd.FileName);
                    Log($"Usuario eligió {chosen}");
                    Db.Configure(chosen);
                    SaveConfig(configPath, chosen, baseDir);
                }
                else { Log("Usuario canceló SaveFileDialog"); return; }
            }
            else
            {
                SaveConfig(configPath, Db.DbPath, baseDir);
            }
        }

        try { Log("EnsureCreated..."); Db.EnsureCreated(); Log("EnsureCreated OK"); }
        catch (Exception ex)
        {
            Log($"EnsureCreated FAIL: {ex}");
            MessageBox.Show("Error inicializando base de datos (SQLite embebido):\n" + ex.Message + "\n\n" + ex.StackTrace + $"\n\nRuta: {Db.DbPath}\nNo necesita instalar SQLite, ya viene dentro del .exe.\n\nRevisa hefesto.log", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Validar que admin existe y logear hash
        try
        {
            var users = Repos.GetUsuarios();
            Log($"Usuarios en DB: {users.Count}");
            foreach (var u in users) Log($" - {u.Username} / {u.Rol}");
        } catch (Exception ex) { Log($"Error listando usuarios: {ex.Message}"); }

        // Login
        LoginForm login;
        try { login = new LoginForm(); }
        catch (Exception ex) { Log($"LoginForm ctor FAIL: {ex}"); MessageBox.Show($"Error creando login: {ex.Message}\n{ex.StackTrace}"); return; }

        var dr = login.ShowDialog();
        Log($"Login dialog result: {dr}");
        if (dr != DialogResult.OK) { Log("Login no OK, saliendo"); return; }
        login.Dispose();

        // MainForm con try/catch para que no se cierre silencioso
        MainForm main;
        try { main = new MainForm(); Log("MainForm creado OK"); }
        catch (Exception ex)
        {
            Log($"MainForm ctor FAIL: {ex}");
            MessageBox.Show($"Error abriendo ventana principal:\n{ex.Message}\n\n{ex.StackTrace}\n\nRevisa hefesto.log", "Hefesto - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        Log("Application.Run(MainForm)");
        Application.Run(main);
        Log("Application exit");
    }

    static void SaveConfig(string path, string dbPath, string baseDir)
    {
        try
        {
            string toStore = dbPath;
            try
            {
                var abs = Path.GetFullPath(dbPath);
                var baseFull = Path.GetFullPath(baseDir);
                if (abs.StartsWith(baseFull, StringComparison.OrdinalIgnoreCase))
                    toStore = Path.GetRelativePath(baseFull, abs);
            } catch { }
            string existingRepo = Updater.Repo;
            try { if (File.Exists(path)) { var j = JsonDocument.Parse(File.ReadAllText(path)); if (j.RootElement.TryGetProperty("GitHubRepo", out var r)) existingRepo = r.GetString() ?? existingRepo; } } catch { }
            if (string.IsNullOrWhiteSpace(existingRepo) || existingRepo.Contains("TU_USUARIO")) existingRepo = "Lanel96/hefesto";
            File.WriteAllText(path, JsonSerializer.Serialize(new { DbPath = toStore, GitHubRepo = existingRepo }, new JsonSerializerOptions { WriteIndented = true }));
        } catch (Exception ex) { Log($"SaveConfig fail: {ex.Message}"); }
    }
    static void SaveConfig(string path, string dbPath) => SaveConfig(path, dbPath, AppDomain.CurrentDomain.BaseDirectory);
}
