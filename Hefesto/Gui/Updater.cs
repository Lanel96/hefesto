using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace Hefesto.Gui;

public static class Updater
{
    // CONFIGURA AQUÍ tu repo después de crearlo en GitHub
    // ej: "tu-usuario/hefesto-taller"
    public static string Repo = "Lanel96/hefesto";

    public static string CurrentVersion => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.1";

    public static async Task<(bool hasUpdate, string latest, string downloadUrl, string notes)> CheckAsync()
    {
        if (Repo.Contains("TU_USUARIO")) return (false, CurrentVersion, "", "Configura Updater.Repo en Updater.cs");
        try
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.UserAgent.ParseAdd("Hefesto-Updater");
            http.Timeout = TimeSpan.FromSeconds(10);
            var url = $"https://api.github.com/repos/{Repo}/releases/latest";
            var json = await http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);
            var tag = doc.RootElement.GetProperty("tag_name").GetString() ?? "";
            var latest = tag.TrimStart('v', 'V');
            var notes = doc.RootElement.TryGetProperty("body", out var b) ? b.GetString() ?? "" : "";
            string downloadUrl = "";
            if (doc.RootElement.TryGetProperty("assets", out var assets) && assets.GetArrayLength() > 0)
            {
                foreach (var a in assets.EnumerateArray())
                {
                    var name = a.GetProperty("name").GetString() ?? "";
                    if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) || name.Contains("Hefesto"))
                    {
                        downloadUrl = a.GetProperty("browser_download_url").GetString() ?? "";
                        break;
                    }
                }
                if (string.IsNullOrEmpty(downloadUrl))
                    downloadUrl = assets[0].GetProperty("browser_download_url").GetString() ?? "";
            }
            bool hasUpdate = IsNewer(latest, CurrentVersion);
            return (hasUpdate, latest, downloadUrl, notes);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Updater error: {ex.Message}");
            return (false, CurrentVersion, "", ex.Message);
        }
    }

    static bool IsNewer(string latest, string current)
    {
        try
        {
            var l = Parse(latest);
            var c = Parse(current);
            for (int i = 0; i < 3; i++) { if (l[i] > c[i]) return true; if (l[i] < c[i]) return false; }
            return false;
        } catch { return false; }
    }
    static int[] Parse(string v)
    {
        var parts = v.Split('.', '-', '+')[0].Split('.');
        var r = new int[3];
        for (int i = 0; i < Math.Min(3, parts.Length); i++) int.TryParse(parts[i], out r[i]);
        return r;
    }

    public static async Task DownloadAndInstallAsync(string downloadUrl, string latestVersion, IProgress<int>? progress = null)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"Hefesto_{latestVersion}.exe");
        using var http = new HttpClient();
        http.DefaultRequestHeaders.UserAgent.ParseAdd("Hefesto-Updater");
        using var resp = await http.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
        resp.EnsureSuccessStatusCode();
        var total = resp.Content.Headers.ContentLength ?? -1L;
        using var stream = await resp.Content.ReadAsStreamAsync();
        using var file = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None);
        var buffer = new byte[81920];
        long readTotal = 0;
        int read;
        while ((read = await stream.ReadAsync(buffer)) > 0)
        {
            await file.WriteAsync(buffer.AsMemory(0, read));
            readTotal += read;
            if (total > 0 && progress != null) progress.Report((int)(readTotal * 100 / total));
        }

        // Crear script .bat que reemplaza el exe actual después de cerrar
        var currentExe = Application.ExecutablePath;
        var batPath = Path.Combine(Path.GetTempPath(), "hefesto_update.bat");
        var bat = $"""
@echo off
echo Actualizando Hefesto a v{latestVersion}...
timeout /t 2 /nobreak >nul
copy /y "{tempPath}" "{currentExe}" >nul
if errorlevel 1 (
  echo Error copiando, intenta ejecutar como administrador
  pause
  exit /b 1
)
echo Actualizado. Iniciando...
start "" "{currentExe}"
del "{tempPath}" >nul 2>&1
del "%~f0" >nul 2>&1
""";
        File.WriteAllText(batPath, bat);
        var psi = new ProcessStartInfo("cmd.exe", $"/c \"{batPath}\"") { UseShellExecute = true, WindowStyle = ProcessWindowStyle.Hidden };
        Process.Start(psi);
        Application.Exit();
    }
}
