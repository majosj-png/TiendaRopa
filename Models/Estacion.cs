namespace TiendaRopa.Models;

public class Estacion
{
    public int Id { get; set; }
    public required string Nombre { get; set; }

    public List<Prenda> Prendas { get; set; } = [];
}
