# Turismo

Proyecto web ASP.NET Core MVC para gestión y consulta de sitios turísticos.

## Requisitos
- .NET SDK 8
- Microsoft Access Database Engine (para OLEDB)

## Configuración de base de datos
1. Crea una base de datos Access en la ruta `data/turismo.accdb`.
2. Asegúrate de que las tablas existan según el modelo de datos.
3. Ajusta la cadena de conexión en `appsettings.json` si es necesario.

## Ejecutar
- Restaurar y ejecutar:
  - `dotnet restore`
  - `dotnet run`

## Módulos iniciales
- Catálogo público de sitios (MVC: controlador `Sitios`).
- CRUD de sitios, ubicaciones, costos, comidas y usuarios con Access.
- Login básico por sesión para administración.

## Acceso
- Ingresar desde el menú superior en `Ingresar`.
- Administra los módulos desde la navegación principal.
