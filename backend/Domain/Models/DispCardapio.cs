namespace Backend.Domain.Models;

// Oferta de um item numa data/intervalo, separada do estoque global de Item
public class DispCardapio
{
    public DateOnly Data { get; set; }
    public int IntervaloId { get; set; }
    public int ItemId { get; set; }
    public bool Disponivel { get; set; }

    public Intervalo Intervalo { get; set; } = null!;
    public Item Item { get; set; } = null!;
}
