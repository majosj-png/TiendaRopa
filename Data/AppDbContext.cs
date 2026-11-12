using Microsoft.EntityFrameworkCore;
using TiendaRopa.Models;

namespace TiendaRopa.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Prenda> Prendas => Set<Prenda>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>(e =>
        {
            e.Property(c => c.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<Prenda>(e =>
        {
            e.Property(p => p.Nombre).HasMaxLength(150);
            e.Property(p => p.Talle).HasMaxLength(10);
            e.Property(p => p.Color).HasMaxLength(40);
            e.Property(p => p.Precio).HasPrecision(12, 2);
        });

        // ---------- Seeding de datos ----------
        // HasData guarda estos registros dentro de la migración,
        // así se insertan al ejecutar "dotnet ef database update".
        // Los Id deben ir escritos a mano.
        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, Nombre = "Remeras" },
            new Categoria { Id = 2, Nombre = "Pantalones" },
            new Categoria { Id = 3, Nombre = "Abrigos" }
        );

        modelBuilder.Entity<Prenda>().HasData(
            new Prenda { Id = 1, Nombre = "Remera básica algodón", Talle = "M",  Color = "Blanco", Precio = 12000m, Stock = 30, CategoriaId = 1 },
            new Prenda { Id = 2, Nombre = "Remera estampada",      Talle = "L",  Color = "Negro",  Precio = 15000m, Stock = 18, CategoriaId = 1 },
            new Prenda { Id = 3, Nombre = "Jean recto",            Talle = "42", Color = "Azul",   Precio = 35000m, Stock = 12, CategoriaId = 2 },
            new Prenda { Id = 4, Nombre = "Jogger",                Talle = "M",  Color = "Gris",   Precio = 22000m, Stock = 20, CategoriaId = 2 },
            new Prenda { Id = 5, Nombre = "Campera de jean",       Talle = "L",  Color = "Celeste",Precio = 48000m, Stock = 6,  CategoriaId = 3 },
            new Prenda { Id = 6, Nombre = "Buzo con capucha",      Talle = "XL", Color = "Verde",  Precio = 32000m, Stock = 10, CategoriaId = 3 }
        );
    }
}
