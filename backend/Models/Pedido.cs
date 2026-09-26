using Backend.Models.Enums;

namespace Backend.Models;

public class Pedido
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public DateOnly Data { get; set; }

    // Null = venda de balcão fora de intervalo
    public int? IntervaloId { get; set; }

    public StatusPedido Status { get; set; }
    public TipoVenda TipoVenda { get; set; }
    public FormaPagamento FormaPagamento { get; set; }
    public decimal Total { get; set; }
    public string CodigoRetirada { get; set; } = string.Empty;
    public bool TemAlertaAlergia { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? EntregueEm { get; set; }
    public DateTime? CanceladoEm { get; set; }

    public Usuario Usuario { get; set; } = null!;
    public Intervalo? Intervalo { get; set; }
    public ICollection<ItemPedido> Itens { get; set; } = new List<ItemPedido>();
}
