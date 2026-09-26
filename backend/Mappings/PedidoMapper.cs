using Backend.DTOs.Pedidos;
using Backend.Models;
using Backend.Services;
using Microsoft.EntityFrameworkCore;

namespace Backend.Mappings;

// Conversão Pedido → PedidoDto, usada por Pedidos e Painel
public static class PedidoMapper
{
    // Carrega tudo que o PedidoDto precisa: comprador, intervalo e itens
    public static IQueryable<Pedido> ComDetalhes(this IQueryable<Pedido> pedidos) =>
        pedidos
            .AsNoTracking()
            .Include(p => p.Usuario)
            .Include(p => p.Intervalo)
            .Include(p => p.Itens).ThenInclude(ip => ip.Item);

    public static PedidoDto ParaDto(this Pedido pedido) => new()
    {
        Id = pedido.Id,
        UsuarioId = pedido.UsuarioId,
        UsuarioNome = pedido.Usuario.Nome,
        Data = pedido.Data,
        Intervalo = pedido.Intervalo?.Nome,
        Status = PedidoService.StatusAtual(pedido),
        TipoVenda = pedido.TipoVenda,
        FormaPagamento = pedido.FormaPagamento,
        Total = pedido.Total,
        CodigoRetirada = pedido.CodigoRetirada,
        TemAlertaAlergia = pedido.TemAlertaAlergia,
        PodeAlterar = PedidoService.PodeAlterar(pedido),
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
