using Hefesto.Core;
using Xunit;

namespace Hefesto.Tests;

public class QaTests : IDisposable
{
    readonly string dbPath;
    public QaTests()
    {
        SQLitePCL.Batteries_V2.Init();
        dbPath = Path.Combine(Path.GetTempPath(), $"hefesto_qa_{Guid.NewGuid():N}.db");
        Db.Configure(dbPath);
        Db.EnsureCreated();
    }
    public void Dispose()
    {
        try { File.Delete(dbPath); File.Delete(dbPath + "-shm"); File.Delete(dbPath + "-wal"); } catch { }
    }

    [Fact]
    public void QA01_Db_SeCreaYAdminExiste()
    {
        Assert.True(File.Exists(dbPath));
        Assert.True(Repos.Validate("admin", "admin123"), "admin/admin123 debe validar");
        Assert.False(Repos.Validate("admin", "wrong"));
        Assert.False(Repos.Validate("noexiste", "admin123"));
        Assert.True(Repos.Validate("ADMIN", "admin123"));
    }

    [Fact]
    public void QA02_Vehiculo_Crud_PorPlaca()
    {
        var v = new Vehiculo("QA-001", "Toyota", "Hilux", 2020, "Cliente QA", "555-0001");
        Repos.UpsertVehiculo(v);
        var list = Repos.GetVehiculos("QA-001");
        Assert.Contains(list, x => x.Placa == "QA-001");
        Repos.UpsertVehiculo(v with { Marca = "Lexus" });
        var upd = Repos.GetVehiculos("QA-001").First(x => x.Placa == "QA-001");
        Assert.Equal("Lexus", upd.Marca);
        var filt = Repos.GetVehiculos("Lexus");
        Assert.NotEmpty(filt);
    }

    [Fact]
    public void QA03_Servicio_Catalogo_PreciosSeleccionables()
    {
        var s = new Servicio(0, $"S-QA-{Guid.NewGuid():N}".Substring(0,10), "Servicio QA", "desc", 99.99m, 45);
        Repos.SaveServicio(s);
        var found = Repos.GetServicios(s.Codigo).First();
        Assert.Equal(99.99m, found.Precio);
        Repos.SaveServicio(found with { Precio = 150m });
        var edited = Repos.GetServicios(s.Codigo).First();
        Assert.Equal(150m, edited.Precio);
    }

    [Fact]
    public void QA04_Orden_ConServiciosYRepuestos_TotalYGarantia()
    {
        var placa = $"QA{Guid.NewGuid():N}".Substring(0, 8).ToUpper();
        Repos.UpsertVehiculo(new Vehiculo(placa, "Mazda", "3", 2019, "Cliente Orden", "555-0002"));
        var svc = Repos.GetServicios().First();
        var orden = new Orden(0, placa, DateTime.Now, null, "Abierta", "QA obs", 0);
        decimal precioAplicado = svc.Precio + 10m;
        int id = Repos.CreateOrden(
            orden,
            new List<(int, string, decimal, int)> { (svc.Id, svc.Nombre, precioAplicado, 2) },
            new List<(string, string, int, DateTime)>
            {
                ("REP-QA-EN", "Filtro Aire", 30, DateTime.Now),
                ("REP-QA-VEN", "Bujia Vieja", 7, DateTime.Now.AddDays(-10))
            });
        Assert.True(id > 0);
        var creada = Repos.GetOrdenes(placa).First(o => o.Id == id);
        Assert.Equal(precioAplicado * 2, creada.Total);
        var reps = Repos.GetRepuestos(id);
        Assert.Equal(2, reps.Count);
        var en = reps.First(r => r.Codigo == "REP-QA-EN");
        var ven = reps.First(r => r.Codigo == "REP-QA-VEN");
        Assert.True(en.EnGarantia);
        Assert.Contains("EN GARANTÍA", en.EstadoGarantia);
        Assert.False(ven.EnGarantia);
        Assert.Contains("VENCIDA", ven.EstadoGarantia);
        var bit = Repos.GetAllRepuestos("REP-QA");
        Assert.True(bit.Count >= 2);
    }

    [Fact]
    public void QA05_Bitacora_EstadoDiasRestantes()
    {
        var rEn = new OrdenRepuesto(0, 1, "X", "Test", 10, DateTime.Now, DateTime.Now.AddDays(10));
        Assert.Contains("10 días", rEn.EstadoGarantia);
        var rVen = new OrdenRepuesto(0, 1, "Y", "Test", 5, DateTime.Now.AddDays(-10), DateTime.Now.AddDays(-5));
        Assert.Equal("GARANTÍA VENCIDA", rVen.EstadoGarantia);
    }

    [Fact]
    public void QA06_Usuarios_AltaYValidacion()
    {
        var user = $"qa{Guid.NewGuid():N}".Substring(0, 8);
        Repos.SaveUsuario(user, "clave123", "Mecánico");
        Assert.True(Repos.Validate(user, "clave123"));
        Assert.False(Repos.Validate(user, "otra"));
        var list = Repos.GetUsuarios();
        Assert.Contains(list, u => u.Username == user);
    }

    [Fact]
    public void QA07_Servicio_PrecioCongelado_NoSeAlteraHistorico()
    {
        var placa = $"QB{Guid.NewGuid():N}".Substring(0, 8).ToUpper();
        Repos.UpsertVehiculo(new Vehiculo(placa, "Ford", "Ranger", 2021, "Cli Hist", "555-0003"));
        var s = new Servicio(0, $"S-HIST-{Guid.NewGuid():N}".Substring(0, 10), "Hist", "", 100m, 60);
        Repos.SaveServicio(s);
        var svc = Repos.GetServicios(s.Codigo).First();
        var orden = new Orden(0, placa, DateTime.Now, null, "Abierta", "", 0);
        int id = Repos.CreateOrden(orden, new List<(int,string,decimal,int)>{(svc.Id, svc.Nombre, 80m, 1)}, new List<(string,string,int,DateTime)>());
        Repos.SaveServicio(svc with { Precio = 999m });
        var servsOrden = Repos.GetOrdenServicios(id);
        Assert.Single(servsOrden);
        Assert.Equal(80m, servsOrden[0].PrecioAplicado);
    }

    [Fact]
    public void QA08_Db_Portable_RelativePath()
    {
        var baseDir = Path.GetTempPath();
        var abs = Path.Combine(baseDir, "hefesto.db");
        var rel = Path.GetRelativePath(baseDir, abs);
        Assert.Equal("hefesto.db", rel);
        var resolved = Path.GetFullPath(Path.Combine(baseDir, rel));
        Assert.Equal(abs, resolved);
    }

    [Fact]
    public void QA09_Orden_Estado_Update()
    {
        var placa = $"QC{Guid.NewGuid():N}".Substring(0, 8).ToUpper();
        Repos.UpsertVehiculo(new Vehiculo(placa, "Kia", "Rio", 2022, "Cli Est", "555-0004"));
        var svc = Repos.GetServicios().First();
        var id = Repos.CreateOrden(new Orden(0, placa, DateTime.Now, null, "Abierta", "", 0), new List<(int,string,decimal,int)>{(svc.Id, svc.Nombre, svc.Precio,1)}, new List<(string,string,int,DateTime)>());
        Repos.UpdateOrdenEstado(id, "Entregada", DateTime.Now);
        var o = Repos.GetOrdenes(placa).First(x=>x.Id==id);
        Assert.Equal("Entregada", o.Estado);
        Assert.NotNull(o.FechaEntrega);
    }
}
