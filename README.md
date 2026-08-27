# Hefesto - Taller Mecánico

Sistema ligero para taller mecánico (1 PC, SQLite embebido). Módulos: Órdenes, Vehículos (placa/marca/modelo), Catálogo Servicios con precios, Repuestos con garantía (código/nombre/días), Bitácora EN GARANTÍA/VENCIDA, Configuración usuarios y DB portable.

## Stack
- C# .NET 10 WinForms + SQLite (WAL, SingleFile SelfContained win-x64)
- DB `hefesto.db` portable junto al exe, se crea al primer inicio. Fallback a `%LocalAppData%\Hefesto` si no hay permiso.
- Actualizaciones vía GitHub Releases (auto-update embebido).

## Desarrollo
```bash
dotnet build Hefesto/Hefesto.slnx -c Release
dotnet publish Hefesto/Gui/Hefesto.Gui.csproj -c Release -o publish-hefesto --self-contained true /p:PublishSingleFile=true
./publish-hefesto/Hefesto.exe # login admin/admin123
```

## Publicar nueva versión
1. Cambia `Version` en `Hefesto/Gui/Hefesto.Gui.csproj` y `Hefesto/Core/Hefesto.Core.csproj` (ej. 0.0.2)
2. Configura `Hefesto/Gui/Updater.cs` -> `Repo = "Lanel96/hefesto"`
3. Commit + tag:
```bash
git add -A && git commit -m "feat: v0.0.2"
git tag v0.0.2 && git push origin main --tags
```
El workflow `.github/workflows/build.yml` compilará el exe y creará el Release en GitHub con `Hefesto.exe` adjunto. El cliente detecta la nueva versión al iniciar y ofrece descargar.

## Primer arranque portable
El `appsettings.json` usa `"DbPath": "hefesto.db"` (relativo al exe). Mueve toda la carpeta `publish-hefesto` donde quieras y seguirá funcionando.

## Licencia
MIT
