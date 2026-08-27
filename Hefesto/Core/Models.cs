namespace Hefesto.Core;

public record Vehiculo(string Placa, string Marca, string Modelo, int? Anio, string Cliente, string Telefono);
public record Servicio(int Id, string Codigo, string Nombre, string Descripcion, decimal Precio, int DuracionMin);
public record Orden(int Id, string Placa, DateTime FechaIngreso, DateTime? FechaEntrega, string Estado, string Observaciones, decimal Total);
public record OrdenServicio(int Id, int OrdenId, int ServicioId, string ServicioNombre, decimal PrecioAplicado, int Cantidad);
public record OrdenRepuesto(int Id, int OrdenId, string Codigo, string Nombre, int DiasGarantia, DateTime FechaInicio, DateTime FechaFin)
{
    public string EstadoGarantia => DateTime.Now.Date <= FechaFin.Date ? $"EN GARANTÍA ({(FechaFin.Date - DateTime.Now.Date).Days} días restantes)" : "GARANTÍA VENCIDA";
    public bool EnGarantia => DateTime.Now.Date <= FechaFin.Date;
}
public record Usuario(int Id, string Username, string PasswordHash, string Rol);
