using Hefesto.Core;
using System.Text.Json;

namespace Hefesto.Gui;

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

        // SQLite embebido - no requiere instalación. Se distribuye dentro del .exe
        try { SQLitePCL.Batteries_V2.Init(); } catch { }

        // Cargar configuración
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
            }
        } catch { }

        string ResolveDbPath(string? configured, string baseDirectory)
        {
            if (string.IsNullOrWhiteSpace(configured)) return Path.Combine(baseDirectory, "hefesto.db");
            var trimmed = configured.Trim();
            // relativo -> combinar con baseDir
            if (!Path.IsPathRooted(trimmed)) return Path.GetFullPath(Path.Combine(baseDirectory, trimmed));
            // absoluto
            var abs = Path.GetFullPath(trimmed);
            var baseFull = Path.GetFullPath(baseDirectory);
            // si está dentro del baseDir actual, respetarlo
            if (abs.StartsWith(baseFull, StringComparison.OrdinalIgnoreCase)) return abs;
            // si es una ruta vieja del proyecto y ya no existe, ignorar y usar baseDir (portable)
            if (!File.Exists(abs) && (abs.Contains("OracleReportGenerator", StringComparison.OrdinalIgnoreCase) || abs.Contains("\\bin\\", StringComparison.OrdinalIgnoreCase) || abs.Contains("publish-hefesto", StringComparison.OrdinalIgnoreCase)))
                return Path.Combine(baseDirectory, "hefesto.db");
            // si el directorio no existe, también fallback a portable
            var dir = Path.GetDirectoryName(abs);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir) && !File.Exists(abs))
                return Path.Combine(baseDirectory, "hefesto.db");
            return abs;
        }

        // Validar ruta escribible, si no, usar LocalAppData
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
            catch
            {
                var fallbackDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Hefesto");
                Directory.CreateDirectory(fallbackDir);
                return Path.Combine(fallbackDir, "hefesto.db");
            }
        }

        var defaultPath = ResolveDbPath(dbPathRaw, baseDir);
        // Si la ruta resuelta es obsoleta y no existe, corregir config automáticamente a relativo
        if (!string.IsNullOrWhiteSpace(dbPathRaw) && !Path.IsPathRooted(dbPathRaw) == false)
        {
            var resolvedCheck = ResolveDbPath(dbPathRaw, baseDir);
            if (resolvedCheck != Path.GetFullPath(dbPathRaw.Trim()) && !File.Exists(Path.GetFullPath(dbPathRaw.Trim())))
            {
                // ruta absoluta vieja que apunta a proyecto -> migrar a relativo
                SaveConfig(configPath, resolvedCheck, baseDir);
                defaultPath = resolvedCheck;
            }
        }
        defaultPath = GetWritablePath(defaultPath);
        Db.Configure(defaultPath);

        // Si no existe DB, ofrecer crear / seleccionar
        if (!File.Exists(Db.DbPath))
        {
            var res = MessageBox.Show(
                $"No se encontró la base de datos en:\n{Db.DbPath}\n\n¿Desea crear una nueva base en esa ubicación?\n\nSí = Crear aquí (SQLite embebido, no requiere instalación)\nNo = Seleccionar otra ubicación\nCancelar = Salir",
                "Hefesto - Primera ejecución", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (res == DialogResult.Cancel) return;
            if (res == DialogResult.No)
            {
                using var sfd = new SaveFileDialog { Filter = "Base Hefesto (*.db)|*.db", FileName = "hefesto.db", Title = "Seleccione ubicación de la base de datos" };
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    var chosen = GetWritablePath(sfd.FileName);
                    Db.Configure(chosen);
                    SaveConfig(configPath, chosen, baseDir);
                }
                else return;
            }
            else
            {
                SaveConfig(configPath, Db.DbPath, baseDir);
            }
        }

        try { Db.EnsureCreated(); }
        catch (Exception ex)
        {
            MessageBox.Show("Error inicializando base de datos (SQLite embebido):\n" + ex.Message + "\n\n" + ex.StackTrace + $"\n\nRuta: {Db.DbPath}\nNo necesita instalar SQLite, ya viene dentro del .exe.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Login
        using var login = new LoginForm();
        if (login.ShowDialog() != DialogResult.OK) return;

        Application.Run(new MainForm());
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
                    toStore = Path.GetRelativePath(baseFull, abs); // portable: "hefesto.db"
            } catch { }
            // preservar GitHubRepo si existe
            string existingRepo = "TU_USUARIO/hefesto";
            try { if (File.Exists(path)) { var j = JsonDocument.Parse(File.ReadAllText(path)); if (j.RootElement.TryGetProperty("GitHubRepo", out var r)) existingRepo = r.GetString() ?? existingRepo; } } catch { }
            if (!string.IsNullOrWhiteSpace(Updater.Repo) && !Updater.Repo.Contains("TU_USUARIO")) existingRepo = Updater.Repo;
            File.WriteAllText(path, JsonSerializer.Serialize(new { DbPath = toStore, GitHubRepo = existingRepo }, new JsonSerializerOptions { WriteIndented = true }));
        } catch { }
    }
    static void SaveConfig(string path, string dbPath) => SaveConfig(path, dbPath, AppDomain.CurrentDomain.BaseDirectory);
}
