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

// ---------- Prendas ----------
var prendas = app.MapGroup("/prendas");

prendas.MapGet("/", async (AppDbContext db) =>
    await db.Prendas
        .OrderBy(p => p.Id)
        .Select(p => new { p.Id, p.Nombre, p.Talle, p.Color, p.Precio, p.Stock, p.CategoriaId, Categoria = p.Categoria!.Nombre })
        .ToListAsync());

prendas.MapGet("/{id:int}", async (int id, AppDbContext db) =>
    await db.Prendas.FindAsync(id) is Prenda p ? Results.Ok(p) : Results.NotFound());

prendas.MapPost("/", async (Prenda prenda, AppDbContext db) =>
{
    if (!await db.Categorias.AnyAsync(c => c.Id == prenda.CategoriaId))
        return Results.BadRequest($"No existe la categoría {prenda.CategoriaId}");

    db.Prendas.Add(prenda);
    await db.SaveChangesAsync();
    return Results.Created($"/prendas/{prenda.Id}", prenda);
});

prendas.MapPut("/{id:int}", async (int id, Prenda datos, AppDbContext db) =>
{
    var p = await db.Prendas.FindAsync(id);
    if (p is null) return Results.NotFound();

    p.Nombre = datos.Nombre;
    p.Talle = datos.Talle;
    p.Color = datos.Color;
    p.Precio = datos.Precio;
    p.Stock = datos.Stock;
    p.CategoriaId = datos.CategoriaId;
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
