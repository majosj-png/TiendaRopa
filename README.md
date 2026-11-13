# TiendaRopa – API mínima con ASP.NET Core + PostgreSQL

API mínima (.NET 10) de una tienda de ropa, con Entity Framework Core, PostgreSQL y datos iniciales (seeding).

## Requisitos
- .NET SDK 10
- PostgreSQL en `localhost:5432`
- Herramienta EF: `dotnet tool install --global dotnet-ef`
- VS Code con la extensión **C# Dev Kit** (y opcional **REST Client** para el archivo `.http`)

## Estructura
```
Models/Categoria.cs, Models/Prenda.cs → entidades
Data/AppDbContext.cs                  → DbContext + seeding (HasData)
Migrations/                           → migración generada con el seed
Program.cs                            → configuración y endpoints
TiendaRopa.http                       → peticiones de prueba
```

## Pasos
1. Pon la contraseña de tu usuario `postgres` en `appsettings.Development.json` → `ConnectionStrings:DefaultConnection`.
2. Ejecuta la API:
   ```
   dotnet run
   ```
   En Development, la API aplica sola las migraciones al arrancar: crea la base `tiendaropa`, las tablas y los datos del seed.
   (También puedes hacerlo a mano con `dotnet ef database update`.)
3. Prueba: http://localhost:5030/prendas — o abre `TiendaRopa.http` y pulsa "Send Request".
4. En VS Code también puedes pulsar **F5** (configuración "API (.NET)").

## Endpoints
| Método | Ruta | Descripción |
|---|---|---|
| GET | /categorias | Lista de categorías con su número de prendas |
| GET | /categorias/{id} | Una categoría |
| GET | /categorias/{id}/prendas | Prendas de una categoría |
| POST | /categorias | Crear categoría |
| GET | /estaciones | Lista de estaciones con su número de prendas |
| GET | /estaciones/{id} | Una estación |
| GET | /estaciones/{id}/prendas | Prendas de una estación |
| POST | /estaciones | Crear estación |
| GET | /prendas | Lista de prendas (incluye categoría y estación) |
| GET | /prendas/{id} | Una prenda |
| POST | /prendas | Crear prenda |
| PUT | /prendas/{id} | Actualizar |
| DELETE | /prendas/{id} | Eliminar |

Documento OpenAPI: http://localhost:5030/openapi/v1.json

## Datos del seed
- Categorías: Remeras, Pantalones, Abrigos
- Estaciones: Verano, Primavera, Otoño, Invierno
- 6 prendas con talla, color, precio, stock, categoría y estación

## Cambiar los datos del seed
1. Edita `HasData(...)` en `Data/AppDbContext.cs`.
2. `dotnet ef migrations add CambioSeed`
3. `dotnet run` (o `dotnet ef database update`).
