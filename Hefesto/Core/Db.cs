using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;

namespace Hefesto.Core;

public static class Db
{
    public static string DbPath { get; private set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "hefesto.db");
    public static string ConnectionString => $"Data Source={DbPath}";

    public static void Configure(string? customPath)
    {
        if (!string.IsNullOrWhiteSpace(customPath))
            DbPath = customPath;
    }

    public static void EnsureCreated()
    {
        // Asegurar directorio existe
        var dir = Path.GetDirectoryName(DbPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        bool exists = File.Exists(DbPath);
        // Si el archivo existe pero es 0 bytes o corrupto, lo tratamos como nuevo
        if (exists)
        {
            var fi = new FileInfo(DbPath);
            if (fi.Length == 0) exists = false;
        }

        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL; PRAGMA foreign_keys=ON;";
            cmd.ExecuteNonQuery();
        }

        // Crear tablas siempre con IF NOT EXISTS (idempotente)
        var sql = @"
CREATE TABLE IF NOT EXISTS Vehiculos (
  Placa TEXT PRIMARY KEY COLLATE NOCASE,
  Marca TEXT NOT NULL,
  Modelo TEXT NOT NULL,
  Anio INTEGER,
  Cliente TEXT NOT NULL,
  Telefono TEXT
);
CREATE TABLE IF NOT EXISTS Servicios (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Codigo TEXT UNIQUE NOT NULL,
  Nombre TEXT NOT NULL,
  Descripcion TEXT,
  Precio REAL NOT NULL,
  DuracionMin INTEGER NOT NULL DEFAULT 60
);
CREATE TABLE IF NOT EXISTS Ordenes (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Placa TEXT NOT NULL REFERENCES Vehiculos(Placa),
  FechaIngreso TEXT NOT NULL,
  FechaEntrega TEXT,
  Estado TEXT NOT NULL DEFAULT 'Abierta',
  Observaciones TEXT,
  Total REAL NOT NULL DEFAULT 0
);
CREATE TABLE IF NOT EXISTS OrdenServicios (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  OrdenId INTEGER NOT NULL REFERENCES Ordenes(Id) ON DELETE CASCADE,
  ServicioId INTEGER NOT NULL REFERENCES Servicios(Id),
  ServicioNombre TEXT NOT NULL,
  PrecioAplicado REAL NOT NULL,
  Cantidad INTEGER NOT NULL DEFAULT 1
);
CREATE TABLE IF NOT EXISTS OrdenRepuestos (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  OrdenId INTEGER NOT NULL REFERENCES Ordenes(Id) ON DELETE CASCADE,
  Codigo TEXT NOT NULL,
  Nombre TEXT NOT NULL,
  DiasGarantia INTEGER NOT NULL,
  FechaInicio TEXT NOT NULL,
  FechaFin TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS Usuarios (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Username TEXT UNIQUE NOT NULL,
  PasswordHash TEXT NOT NULL,
  Rol TEXT NOT NULL DEFAULT 'Admin'
);
CREATE INDEX IF NOT EXISTS idx_ordenes_placa ON Ordenes(Placa);
CREATE INDEX IF NOT EXISTS idx_repuestos_orden ON OrdenRepuestos(OrdenId);
";
        using var cmd2 = conn.CreateCommand();
        cmd2.CommandText = sql;
        cmd2.ExecuteNonQuery();

        // Asegurar admin existe (siempre)
        var hash = Hash("admin123");
        using var cmdCheck = conn.CreateCommand();
        cmdCheck.CommandText = "SELECT COUNT(*) FROM Usuarios WHERE Username='admin' COLLATE NOCASE";
        var cnt = Convert.ToInt32(cmdCheck.ExecuteScalar());
        if (cnt == 0)
        {
            using var cmd3 = conn.CreateCommand();
            cmd3.CommandText = "INSERT INTO Usuarios (Username, PasswordHash, Rol) VALUES ('admin', $h, 'Admin')";
            cmd3.Parameters.AddWithValue("$h", hash);
            cmd3.ExecuteNonQuery();
        }
        else
        {
            // reparar hash si es diferente (migración)
            using var cmdFix = conn.CreateCommand();
            cmdFix.CommandText = "SELECT PasswordHash FROM Usuarios WHERE Username='admin' COLLATE NOCASE";
            var existing = cmdFix.ExecuteScalar() as string;
            if (existing != hash)
            {
                using var cmdUpd = conn.CreateCommand();
                cmdUpd.CommandText = "UPDATE Usuarios SET PasswordHash=$h WHERE Username='admin' COLLATE NOCASE";
                cmdUpd.Parameters.AddWithValue("$h", hash);
                cmdUpd.ExecuteNonQuery();
            }
        }

        // Servicios de ejemplo solo si tabla vacía
        using var cmdCnt = conn.CreateCommand();
        cmdCnt.CommandText = "SELECT COUNT(*) FROM Servicios";
        var scnt = Convert.ToInt32(cmdCnt.ExecuteScalar());
        if (scnt == 0)
        {
            using var cmd4 = conn.CreateCommand();
            cmd4.CommandText = @"
INSERT OR IGNORE INTO Servicios (Codigo, Nombre, Descripcion, Precio, DuracionMin) VALUES
 ('S001','Cambio de Aceite','Aceite 5W30 + filtro', 45.00, 30),
 ('S002','Alineación y Balanceo','Alineación 4 ruedas', 60.00, 60),
 ('S003','Frenos Delanteros','Pastillas + rectificado', 120.00, 90),
 ('S004','Diagnóstico Computarizado','Scanner OBD2', 25.00, 20),
 ('S005','Cambio Correa Distribución','Kit completo', 250.00, 180);
";
            cmd4.ExecuteNonQuery();
        }
    }

    public static string Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }

    public static SqliteConnection Open() { var c = new SqliteConnection(ConnectionString); c.Open(); using var cmd=c.CreateCommand(); cmd.CommandText="PRAGMA foreign_keys=ON;"; cmd.ExecuteNonQuery(); return c; }
}
