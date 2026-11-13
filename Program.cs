using Microsoft.EntityFrameworkCore;
using TiendaRopa.Data;
using TiendaRopa.Models;

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL con EF Core (cadena de conexión en appsettings.json o variable de entorno ConnectionStrings__DefaultConnection)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // http://localhost:5030/openapi/v1.json (solo en desarrollo)
}

// Aplica las migraciones pendientes al arrancar (incluye el seeding definido en las migraciones).
// Reintenta para tolerar que la base de datos tarde en estar disponible (p. ej. en Docker Compose).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var retries = 10;
    while (true)
    {
        try
        {
            db.Database.Migrate();
            break;
        }
        catch when (retries-- > 0)
        {
            app.Logger.LogWarning("No se pudo conectar/migrar la base de datos. Reintento en 3s ({Retries} restantes)...", retries);
            Thread.Sleep(TimeSpan.FromSeconds(3));
        }
    }
}

app.MapGet("/", () => Results.Ok(new { status = "ok", service = "TiendaRopa API" }));

// ---------- Categorías ----------
var categorias = app.MapGroup("/categorias");

categorias.MapGet("/", async (AppDbContext db) =>
    await db.Categorias
        .OrderBy(c => c.Id)
        .Select(c => new { c.Id, c.Nombre, TotalPrendas = c.Prendas.Count })
        .ToListAsync());

categorias.MapGet("/{id:int}", async (int id, AppDbContext db) =>
    await db.Categorias.FindAsync(id) is Categoria c ? Results.Ok(c) : Results.NotFound());

categorias.MapGet("/{id:int}/prendas", async (int id, AppDbContext db) =>
    await db.Prendas
        .Where(p => p.CategoriaId == id)
        .OrderBy(p => p.Id)
        .Select(p => new { p.Id, p.Nombre, p.Talle, p.Color, p.Precio, p.Stock })
        .ToListAsync());

categorias.MapPost("/", async (Categoria categoria, AppDbContext db) =>
{
    db.Categorias.Add(categoria);
    await db.SaveChangesAsync();
    return Results.Created($"/categorias/{categoria.Id}", categoria);
});

// ---------- Estaciones ----------
var estaciones = app.MapGroup("/estaciones");

estaciones.MapGet("/", async (AppDbContext db) =>
    await db.Estaciones
        .OrderBy(e => e.Id)
        .Select(e => new { e.Id, e.Nombre, TotalPrendas = e.Prendas.Count })
        .ToListAsync());

estaciones.MapGet("/{id:int}", async (int id, AppDbContext db) =>
    await db.Estaciones
        .Where(e => e.Id == id)
        .Select(e => new { e.Id, e.Nombre, TotalPrendas = e.Prendas.Count })
        .FirstOrDefaultAsync() is { } e
        ? Results.Ok(e)
        : Results.NotFound());

estaciones.MapGet("/{id:int}/prendas", async (int id, AppDbContext db) =>
    await db.Prendas
        .Where(p => p.EstacionId == id)
        .OrderBy(p => p.Id)
        .Select(p => new { p.Id, p.Nombre, p.Talle, p.Color, p.Precio, p.Stock })
        .ToListAsync());

estaciones.MapPost("/", async (Estacion estacion, AppDbContext db) =>
{
    db.Estaciones.Add(estacion);
    await db.SaveChangesAsync();
    return Results.Created($"/estaciones/{estacion.Id}", estacion);
});

// ---------- Prendas ----------
var prendas = app.MapGroup("/prendas");

prendas.MapGet("/", async (AppDbContext db) =>
    await db.Prendas
        .OrderBy(p => p.Id)
        .Select(p => new
        {
            p.Id,
            p.Nombre,
            p.Talle,
            p.Color,
            p.Precio,
            p.Stock,
            p.CategoriaId,
            Categoria = p.Categoria!.Nombre,
            p.EstacionId,
            Estacion = p.Estacion!.Nombre
        })
        .ToListAsync());

prendas.MapGet("/{id:int}", async (int id, AppDbContext db) =>
    await db.Prendas
        .Where(p => p.Id == id)
        .Select(p => new
        {
            p.Id,
            p.Nombre,
            p.Talle,
            p.Color,
            p.Precio,
            p.Stock,
            p.CategoriaId,
            Categoria = p.Categoria!.Nombre,
            p.EstacionId,
            Estacion = p.Estacion!.Nombre
        })
        .FirstOrDefaultAsync() is { } p
        ? Results.Ok(p)
        : Results.NotFound());

prendas.MapPost("/", async (Prenda prenda, AppDbContext db) =>
{
    if (!await db.Categorias.AnyAsync(c => c.Id == prenda.CategoriaId))
        return Results.BadRequest($"No existe la categoría {prenda.CategoriaId}");

    if (!await db.Estaciones.AnyAsync(e => e.Id == prenda.EstacionId))
        return Results.BadRequest($"No existe la estación {prenda.EstacionId}");

    db.Prendas.Add(prenda);
    await db.SaveChangesAsync();

    var creada = await db.Prendas
        .Where(p => p.Id == prenda.Id)
        .Select(p => new
        {
            p.Id,
            p.Nombre,
            p.Talle,
            p.Color,
            p.Precio,
            p.Stock,
            p.CategoriaId,
            Categoria = p.Categoria!.Nombre,
            p.EstacionId,
            Estacion = p.Estacion!.Nombre
        })
        .FirstAsync();

    return Results.Created($"/prendas/{prenda.Id}", creada);
});

prendas.MapPut("/{id:int}", async (int id, Prenda datos, AppDbContext db) =>
{
    var p = await db.Prendas.FindAsync(id);
    if (p is null) return Results.NotFound();

    if (!await db.Categorias.AnyAsync(c => c.Id == datos.CategoriaId))
        return Results.BadRequest($"No existe la categoría {datos.CategoriaId}");

    if (!await db.Estaciones.AnyAsync(e => e.Id == datos.EstacionId))
        return Results.BadRequest($"No existe la estación {datos.EstacionId}");

    p.Nombre = datos.Nombre;
    p.Talle = datos.Talle;
    p.Color = datos.Color;
    p.Precio = datos.Precio;
    p.Stock = datos.Stock;
    p.CategoriaId = datos.CategoriaId;
    p.EstacionId = datos.EstacionId;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

prendas.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
{
    var p = await db.Prendas.FindAsync(id);
    if (p is null) return Results.NotFound();

    db.Prendas.Remove(p);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
