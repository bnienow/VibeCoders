using Backend.Domain.Models.Enums;

namespace Backend.Domain.Models;

public class Movimento
{
    public int Id { get; set; }
    public int ContaId { get; set; }
    public TipoMovimento Tipo { get; set; }

    // Negativo em Compra; positivo em Credito, Pagamento e Estorno
    public decimal Valor { get; set; }

    public DateTime Data { get; set; }

    // Null em crédito/pagamento
    public int? PedidoId { get; set; }

    public string Descricao { get; set; } = string.Empty;
    public decimal SaldoApos { get; set; }

    public Conta Conta { get; set; } = null!;
    public Pedido? Pedido { get; set; }
}
