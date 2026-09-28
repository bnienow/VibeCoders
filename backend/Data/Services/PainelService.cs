using Backend.Domain.DTOs.Painel;
using Backend.Domain.DTOs.Pedidos;
using Backend.Domain.Interfaces.Repositories;
using Backend.Domain.Interfaces.Services;
using Backend.Domain.Mappings;
using Backend.Domain.Models;
using Backend.Domain.Models.Enums;

namespace Backend.Data.Services;

public class PainelService : IPainelService
{
    private readonly IIntervaloRepository _intervalos;
    private readonly IPedidoRepository _pedidos;

    public PainelService(IIntervaloRepository intervalos, IPedidoRepository pedidos)
    {
        _intervalos = intervalos;
        _pedidos = pedidos;
    }

    public async Task<PainelDto> IntervaloAtual()
    {
        var intervalo = await ProximoIntervalo();
        if (intervalo == null)
            return new PainelDto();

        var pedidos = await PedidosDoIntervalo(intervalo);

        return new PainelDto
        {
            Intervalo = intervalo.Nome,
            HoraInicio = intervalo.HoraInicio,
            HoraFim = intervalo.HoraFim,
            Pendentes = pedidos.Count(p => p.Status != StatusPedido.Entregue),
            Entregues = pedidos.Count(p => p.Status == StatusPedido.Entregue),
            Pedidos = pedidos.Select(p => p.ParaDto()).ToList(),
        };
    }

    public async Task<List<PreparoItemDto>> Preparo()
    {
        var intervalo = await ProximoIntervalo();
        if (intervalo == null)
            return [];

        var pedidos = await PedidosDoIntervalo(intervalo);

        return pedidos
            .Where(p => p.Status != StatusPedido.Entregue)
            .SelectMany(p => p.Itens)
            .GroupBy(ip => ip.Item.Nome)
            .Select(g => new PreparoItemDto { Nome = g.Key, Quantidade = g.Sum(ip => ip.Quantidade) })
            .OrderByDescending(i => i.Quantidade)
            .ToList();
    }

    public async Task<List<PedidoDto>> Buscar(string termo)
    {
        var pedidos = await _pedidos.BuscarAntecipados(Hoje(), termo);

        return pedidos.Select(p => p.ParaDto()).ToList();
    }

    // Intervalo de hoje que ainda não terminou, o mais cedo primeiro
    private Task<Intervalo?> ProximoIntervalo() =>
        _intervalos.Proximo(TimeOnly.FromDateTime(DateTime.Now));

    private Task<List<Pedido>> PedidosDoIntervalo(Intervalo intervalo) =>
        _pedidos.AntecipadosDoIntervalo(Hoje(), intervalo.Id);

    private static DateOnly Hoje() => DateOnly.FromDateTime(DateTime.Now);
}
