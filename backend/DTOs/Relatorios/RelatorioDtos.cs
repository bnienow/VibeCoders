namespace Backend.DTOs.Relatorios;

// Vendas de um dia
public class VendaDiaDto
{
    public DateOnly Data { get; set; }
    public int Pedidos { get; set; }
    public decimal Total { get; set; }
}

// Uma linha do ranking de itens
public class ItemVendidoDto
{
    public string Nome { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal Total { get; set; }
}
