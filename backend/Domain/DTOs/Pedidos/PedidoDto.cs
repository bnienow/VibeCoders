using Backend.Domain.Models.Enums;

namespace Backend.Domain.DTOs.Pedidos;

// O que a API devolve de um pedido
public class PedidoDto
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string UsuarioNome { get; set; } = string.Empty;
    public DateOnly Data { get; set; }
    public int? IntervaloId { get; set; }
    public string? Intervalo { get; set; }
    public StatusPedido Status { get; set; }
    public TipoVenda TipoVenda { get; set; }
    public FormaPagamento FormaPagamento { get; set; }
    public decimal Total { get; set; }
    public string CodigoRetirada { get; set; } = string.Empty;
    public bool TemAlertaAlergia { get; set; }

    // True enquanto a janela está aberta: o front mostra os botões Alterar e Cancelar
    public bool PodeAlterar { get; set; }

    public DateTime CriadoEm { get; set; }
    public DateTime? EntregueEm { get; set; }
    public List<ItemPedidoDto> Itens { get; set; } = [];
}

public class ItemPedidoDto
{
    public int ItemId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal Subtotal { get; set; }
}
