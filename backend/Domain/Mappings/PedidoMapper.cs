using Backend.Domain.DTOs.Pedidos;
using Backend.Domain.Models;

namespace Backend.Domain.Mappings;

// Conversão Pedido → PedidoDto, usada por Pedidos e Painel
public static class PedidoMapper
{
    public static PedidoDto ParaDto(this Pedido pedido) => new()
    {
        Id = pedido.Id,
        UsuarioId = pedido.UsuarioId,
        UsuarioNome = pedido.Usuario.Nome,
        Data = pedido.Data,
        IntervaloId = pedido.IntervaloId,
        Intervalo = pedido.Intervalo?.Nome,
        Status = pedido.StatusAtual(),
        TipoVenda = pedido.TipoVenda,
        FormaPagamento = pedido.FormaPagamento,
        Total = pedido.Total,
        CodigoRetirada = pedido.CodigoRetirada,
        TemAlertaAlergia = pedido.TemAlertaAlergia,
        PodeAlterar = pedido.PodeAlterar(),
        CriadoEm = pedido.CriadoEm,
        EntregueEm = pedido.EntregueEm,
        Itens = pedido.Itens.Select(ip => new ItemPedidoDto
        {
            ItemId = ip.ItemId,
            Nome = ip.Item.Nome,
            Quantidade = ip.Quantidade,
            PrecoUnitario = ip.PrecoUnitario,
            Subtotal = ip.Subtotal,
        }).ToList(),
    };
}
