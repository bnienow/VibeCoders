namespace Backend.Domain.DTOs.Relatorios;

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

// Os dois relatórios de um período (base da planilha Excel)
public class RelatorioVendasDto
{
    public DateOnly De { get; set; }
    public DateOnly Ate { get; set; }
    public List<VendaDiaDto> Vendas { get; set; } = [];
    public List<ItemVendidoDto> Itens { get; set; } = [];
}
