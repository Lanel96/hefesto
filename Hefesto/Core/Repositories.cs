using Microsoft.Data.Sqlite;

namespace Hefesto.Core;

public static class Repos
{
    // Vehiculos
    public static List<Vehiculo> GetVehiculos(string? filtro=null)
    {
        var list = new List<Vehiculo>();
        using var c = Db.Open();
        using var cmd = c.CreateCommand();
        if (string.IsNullOrWhiteSpace(filtro))
            cmd.CommandText = "SELECT Placa, Marca, Modelo, Anio, Cliente, Telefono FROM Vehiculos ORDER BY Placa";
        else
        {
            cmd.CommandText = "SELECT Placa, Marca, Modelo, Anio, Cliente, Telefono FROM Vehiculos WHERE Placa LIKE $f OR Marca LIKE $f OR Modelo LIKE $f OR Cliente LIKE $f ORDER BY Placa";
            cmd.Parameters.AddWithValue("$f", $"%{filtro}%");
        }
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(new Vehiculo(r.GetString(0), r.GetString(1), r.GetString(2), r.IsDBNull(3)?null:r.GetInt32(3), r.GetString(4), r.IsDBNull(5)?"":r.GetString(5)));
        return list;
    }
    public static void UpsertVehiculo(Vehiculo v)
    {
        using var c = Db.Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "INSERT INTO Vehiculos (Placa, Marca, Modelo, Anio, Cliente, Telefono) VALUES ($p,$m,$mo,$a,$cl,$t) ON CONFLICT(Placa) DO UPDATE SET Marca=$m, Modelo=$mo, Anio=$a, Cliente=$cl, Telefono=$t";
        cmd.Parameters.AddWithValue("$p", v.Placa.ToUpper().Trim());
        cmd.Parameters.AddWithValue("$m", v.Marca);
        cmd.Parameters.AddWithValue("$mo", v.Modelo);
        cmd.Parameters.AddWithValue("$a", (object?)v.Anio ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$cl", v.Cliente);
        cmd.Parameters.AddWithValue("$t", v.Telefono);
        cmd.ExecuteNonQuery();
    }
    public static void DeleteVehiculo(string placa) { using var c=Db.Open(); using var cmd=c.CreateCommand(); cmd.CommandText="DELETE FROM Vehiculos WHERE Placa=$p"; cmd.Parameters.AddWithValue("$p", placa); cmd.ExecuteNonQuery(); }

    // Servicios
    public static List<Servicio> GetServicios(string? filtro=null)
    {
        var list=new List<Servicio>();
        using var c=Db.Open(); using var cmd=c.CreateCommand();
        if(string.IsNullOrWhiteSpace(filtro)) cmd.CommandText="SELECT Id,Codigo,Nombre,Descripcion,Precio,DuracionMin FROM Servicios ORDER BY Codigo";
        else { cmd.CommandText="SELECT Id,Codigo,Nombre,Descripcion,Precio,DuracionMin FROM Servicios WHERE Codigo LIKE $f OR Nombre LIKE $f ORDER BY Codigo"; cmd.Parameters.AddWithValue("$f",$"%{filtro}%"); }
        using var r=cmd.ExecuteReader();
        while(r.Read()) list.Add(new Servicio(r.GetInt32(0), r.GetString(1), r.GetString(2), r.IsDBNull(3)?"":r.GetString(3), Convert.ToDecimal(r.GetDouble(4)), r.GetInt32(5)));
        return list;
    }
    public static void SaveServicio(Servicio s)
    {
        using var c=Db.Open(); using var cmd=c.CreateCommand();
        if(s.Id==0) { cmd.CommandText="INSERT INTO Servicios (Codigo,Nombre,Descripcion,Precio,DuracionMin) VALUES ($c,$n,$d,$p,$du)"; }
        else { cmd.CommandText="UPDATE Servicios SET Codigo=$c, Nombre=$n, Descripcion=$d, Precio=$p, DuracionMin=$du WHERE Id=$id"; cmd.Parameters.AddWithValue("$id", s.Id); }
        cmd.Parameters.AddWithValue("$c", s.Codigo); cmd.Parameters.AddWithValue("$n", s.Nombre); cmd.Parameters.AddWithValue("$d", s.Descripcion); cmd.Parameters.AddWithValue("$p", (double)s.Precio); cmd.Parameters.AddWithValue("$du", s.DuracionMin);
        cmd.ExecuteNonQuery();
    }
    public static void DeleteServicio(int id){ using var c=Db.Open(); using var cmd=c.CreateCommand(); cmd.CommandText="DELETE FROM Servicios WHERE Id=$id"; cmd.Parameters.AddWithValue("$id", id); cmd.ExecuteNonQuery(); }

    // Ordenes
    public static int CreateOrden(Orden o, List<(int servicioId, string nombre, decimal precio, int cant)> servicios, List<(string codigo,string nombre,int dias,DateTime inicio)> repuestos)
    {
        using var c=Db.Open(); using var tx=c.BeginTransaction();
        using var cmd=c.CreateCommand(); cmd.Transaction=tx;
        var total = servicios.Sum(s=>s.precio*s.cant);
        cmd.CommandText="INSERT INTO Ordenes (Placa,FechaIngreso,FechaEntrega,Estado,Observaciones,Total) VALUES ($p,$fi,$fe,$e,$obs,$t); SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("$p", o.Placa); cmd.Parameters.AddWithValue("$fi", o.FechaIngreso.ToString("o")); cmd.Parameters.AddWithValue("$fe", (object?)o.FechaEntrega?.ToString("o") ?? DBNull.Value); cmd.Parameters.AddWithValue("$e", o.Estado); cmd.Parameters.AddWithValue("$obs", o.Observaciones); cmd.Parameters.AddWithValue("$t", (double)total);
        var id = Convert.ToInt32(cmd.ExecuteScalar());
        foreach(var s in servicios)
        {
            using var cmd2=c.CreateCommand(); cmd2.Transaction=tx;
            cmd2.CommandText="INSERT INTO OrdenServicios (OrdenId,ServicioId,ServicioNombre,PrecioAplicado,Cantidad) VALUES ($o,$s,$n,$p,$cant)";
            cmd2.Parameters.AddWithValue("$o", id); cmd2.Parameters.AddWithValue("$s", s.servicioId); cmd2.Parameters.AddWithValue("$n", s.nombre); cmd2.Parameters.AddWithValue("$p", (double)s.precio); cmd2.Parameters.AddWithValue("$cant", s.cant);
            cmd2.ExecuteNonQuery();
        }
        foreach(var r in repuestos)
        {
            var fin = r.inicio.Date.AddDays(r.dias);
            using var cmd2=c.CreateCommand(); cmd2.Transaction=tx;
            cmd2.CommandText="INSERT INTO OrdenRepuestos (OrdenId,Codigo,Nombre,DiasGarantia,FechaInicio,FechaFin) VALUES ($o,$c,$n,$d,$fi,$ff)";
            cmd2.Parameters.AddWithValue("$o", id); cmd2.Parameters.AddWithValue("$c", r.codigo); cmd2.Parameters.AddWithValue("$n", r.nombre); cmd2.Parameters.AddWithValue("$d", r.dias); cmd2.Parameters.AddWithValue("$fi", r.inicio.ToString("o")); cmd2.Parameters.AddWithValue("$ff", fin.ToString("o"));
            cmd2.ExecuteNonQuery();
        }
        tx.Commit(); return id;
    }
    public static List<Orden> GetOrdenes(string? filtro=null)
    {
        var list=new List<Orden>();
        using var c=Db.Open(); using var cmd=c.CreateCommand();
        if(string.IsNullOrWhiteSpace(filtro)) cmd.CommandText="SELECT Id,Placa,FechaIngreso,FechaEntrega,Estado,Observaciones,Total FROM Ordenes ORDER BY Id DESC";
        else { cmd.CommandText="SELECT Id,Placa,FechaIngreso,FechaEntrega,Estado,Observaciones,Total FROM Ordenes WHERE Placa LIKE $f OR Estado LIKE $f ORDER BY Id DESC"; cmd.Parameters.AddWithValue("$f",$"%{filtro}%"); }
        using var r=cmd.ExecuteReader();
        while(r.Read()) list.Add(new Orden(r.GetInt32(0), r.GetString(1), DateTime.Parse(r.GetString(2)), r.IsDBNull(3)?null:DateTime.Parse(r.GetString(3)), r.GetString(4), r.IsDBNull(5)?"":r.GetString(5), Convert.ToDecimal(r.GetDouble(6))));
        return list;
    }
    public static List<OrdenServicio> GetOrdenServicios(int ordenId)
    {
        var list=new List<OrdenServicio>();
        using var c=Db.Open(); using var cmd=c.CreateCommand(); cmd.CommandText="SELECT Id,OrdenId,ServicioId,ServicioNombre,PrecioAplicado,Cantidad FROM OrdenServicios WHERE OrdenId=$o"; cmd.Parameters.AddWithValue("$o", ordenId);
        using var r=cmd.ExecuteReader(); while(r.Read()) list.Add(new OrdenServicio(r.GetInt32(0), r.GetInt32(1), r.GetInt32(2), r.GetString(3), Convert.ToDecimal(r.GetDouble(4)), r.GetInt32(5)));
        return list;
    }
    public static List<OrdenRepuesto> GetRepuestos(int ordenId)
    {
        var list=new List<OrdenRepuesto>();
        using var c=Db.Open(); using var cmd=c.CreateCommand(); cmd.CommandText="SELECT Id,OrdenId,Codigo,Nombre,DiasGarantia,FechaInicio,FechaFin FROM OrdenRepuestos WHERE OrdenId=$o"; cmd.Parameters.AddWithValue("$o", ordenId);
        using var r=cmd.ExecuteReader(); while(r.Read()) list.Add(new OrdenRepuesto(r.GetInt32(0), r.GetInt32(1), r.GetString(2), r.GetString(3), r.GetInt32(4), DateTime.Parse(r.GetString(5)), DateTime.Parse(r.GetString(6))));
        return list;
    }
    public static List<OrdenRepuesto> GetAllRepuestos(string? filtro=null)
    {
        var list=new List<OrdenRepuesto>();
        using var c=Db.Open(); using var cmd=c.CreateCommand();
        if(string.IsNullOrWhiteSpace(filtro)) cmd.CommandText="SELECT Id,OrdenId,Codigo,Nombre,DiasGarantia,FechaInicio,FechaFin FROM OrdenRepuestos ORDER BY FechaFin DESC";
        else { cmd.CommandText="SELECT Id,OrdenId,Codigo,Nombre,DiasGarantia,FechaInicio,FechaFin FROM OrdenRepuestos WHERE Codigo LIKE $f OR Nombre LIKE $f ORDER BY FechaFin DESC"; cmd.Parameters.AddWithValue("$f",$"%{filtro}%"); }
        using var r=cmd.ExecuteReader(); while(r.Read()) list.Add(new OrdenRepuesto(r.GetInt32(0), r.GetInt32(1), r.GetString(2), r.GetString(3), r.GetInt32(4), DateTime.Parse(r.GetString(5)), DateTime.Parse(r.GetString(6))));
        return list;
    }
    public static void UpdateOrdenEstado(int id, string estado, DateTime? entrega)
    {
        using var c=Db.Open(); using var cmd=c.CreateCommand(); cmd.CommandText="UPDATE Ordenes SET Estado=$e, FechaEntrega=$f WHERE Id=$id"; cmd.Parameters.AddWithValue("$e", estado); cmd.Parameters.AddWithValue("$f", (object?)entrega?.ToString("o") ?? DBNull.Value); cmd.Parameters.AddWithValue("$id", id); cmd.ExecuteNonQuery();
    }
    public static void DeleteOrden(int id){ using var c=Db.Open(); using var cmd=c.CreateCommand(); cmd.CommandText="DELETE FROM Ordenes WHERE Id=$id"; cmd.Parameters.AddWithValue("$id", id); cmd.ExecuteNonQuery(); }

    // Usuarios
    public static bool Validate(string user, string pass)
    {
        using var c=Db.Open(); using var cmd=c.CreateCommand(); cmd.CommandText="SELECT PasswordHash FROM Usuarios WHERE Username=$u COLLATE NOCASE"; cmd.Parameters.AddWithValue("$u", user);
        var h = cmd.ExecuteScalar() as string; return h != null && h == Db.Hash(pass);
    }
    public static List<Usuario> GetUsuarios(){ var l=new List<Usuario>(); using var c=Db.Open(); using var cmd=c.CreateCommand(); cmd.CommandText="SELECT Id,Username,PasswordHash,Rol FROM Usuarios"; using var r=cmd.ExecuteReader(); while(r.Read()) l.Add(new Usuario(r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetString(3))); return l; }
    public static void SaveUsuario(string username, string password, string rol)
    {
        using var c=Db.Open(); using var cmd=c.CreateCommand();
        var exists = c.CreateCommand(); exists.CommandText="SELECT COUNT(*) FROM Usuarios WHERE Username=$u COLLATE NOCASE"; exists.Parameters.AddWithValue("$u", username);
        var cnt = Convert.ToInt32(exists.ExecuteScalar());
        if(cnt>0){ cmd.CommandText="UPDATE Usuarios SET PasswordHash=$h, Rol=$r WHERE Username=$u COLLATE NOCASE"; } else { cmd.CommandText="INSERT INTO Usuarios (Username,PasswordHash,Rol) VALUES ($u,$h,$r)"; }
        cmd.Parameters.AddWithValue("$u", username); cmd.Parameters.AddWithValue("$h", Db.Hash(password)); cmd.Parameters.AddWithValue("$r", rol); cmd.ExecuteNonQuery();
    }
    public static void DeleteUsuario(int id){ using var c=Db.Open(); using var cmd=c.CreateCommand(); cmd.CommandText="DELETE FROM Usuarios WHERE Id=$id"; cmd.Parameters.AddWithValue("$id", id); cmd.ExecuteNonQuery(); }
}
