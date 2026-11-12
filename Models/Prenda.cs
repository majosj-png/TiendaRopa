namespace TiendaRopa.Models;

public class Prenda
{
    public int Id { get; set; }
    public required string Nombre { get; set; }
    public required string Talle { get; set; }   // S, M, L, XL, 38, 40...
    public required string Color { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }

    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }
}
