# Turismo

Aplicacion ASP.NET Core MVC para catalogar sitios turisticos, ubicaciones, costos, comidas, imagenes y usuarios administrativos.

## Ejecutar local

```powershell
dotnet restore
dotnet run
```

La app usa SQLite por defecto. En el primer arranque crea `data/turismo.sqlite` automaticamente usando:

- `data/sqlite-schema.sql`
- `data/sqlite-seed.sql`

## Acceso administrador

- URL local: `http://localhost:5269` o el puerto que indique `dotnet run`.
- Usuario: `admin@turismo.local`
- Clave: `Admin2628`

## Despliegue en Render

Este proyecto ya incluye:

- `Dockerfile`
- `render.yaml`
- SQLite compatible con Linux
- Seed de datos completo
- Imagenes locales en `wwwroot/uploads`

Pasos:

1. Sube el proyecto a GitHub.
2. En Render, crea un nuevo `Blueprint` o `Web Service` desde el repositorio.
3. Si usas Blueprint, Render detectara `render.yaml`.
4. Si lo creas manualmente, selecciona `Docker` como entorno.
5. No agregues base Access: Render no soporta OLEDB/Access en Linux.

La variable `DatabaseProvider=Sqlite` ya esta definida en `render.yaml`.

## Nota de persistencia

Render Free puede reiniciar el filesystem del servicio. Para una demo esta bien porque la base se vuelve a sembrar desde `sqlite-seed.sql`. Para cambios permanentes hechos desde el panel admin, conviene agregar un disco persistente de Render o migrar a PostgreSQL.
