using Backend.Domain.DTOs.Relatorios;
using Backend.Domain.Interfaces.Repositories;
using Backend.Domain.Interfaces.Services;

namespace Backend.Data.Services;

public class RelatorioService : IRelatorioService
{
    private readonly IPedidoRepository _pedidos;

    public RelatorioService(IPedidoRepository pedidos)
    {
        _pedidos = pedidos;
    }

    public Task<List<VendaDiaDto>> VendasPorDia(DateOnly? inicio, DateOnly? fim)
    {
        var (de, ate) = Periodo(inicio, fim);

        return _pedidos.VendasPorDia(de, ate);
    }

    public Task<List<ItemVendidoDto>> RankingDeItens(DateOnly? inicio, DateOnly? fim)
    {
        var (de, ate) = Periodo(inicio, fim);

        return _pedidos.RankingDeItens(de, ate);
    }

    public async Task<RelatorioVendasDto> Completo(DateOnly? inicio, DateOnly? fim)
    {
        var (de, ate) = Periodo(inicio, fim);

        return new RelatorioVendasDto
        {
            De = de,
            Ate = ate,
            Vendas = await _pedidos.VendasPorDia(de, ate),
            Itens = await _pedidos.RankingDeItens(de, ate),
        };
    }

    // Sem datas: dos últimos 30 dias até hoje
    private static (DateOnly De, DateOnly Ate) Periodo(DateOnly? inicio, DateOnly? fim)
    {
        var ate = fim ?? DateOnly.FromDateTime(DateTime.Now);

        return (inicio ?? ate.AddDays(-30), ate);
    }
}
