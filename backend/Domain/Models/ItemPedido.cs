namespace Backend.Domain.Models;

public class ItemPedido
{
    public int PedidoId { get; set; }
    public int ItemId { get; set; }
    public int Quantidade { get; set; }

    // Congelado no momento do pedido — mudança de preço não altera o histórico
    public decimal PrecoUnitario { get; set; }

    public decimal Subtotal { get; set; }

    public Pedido Pedido { get; set; } = null!;
    public Item Item { get; set; } = null!;
}
