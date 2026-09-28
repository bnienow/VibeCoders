using Backend.Domain.DTOs.Relatorios;

namespace Backend.Domain.Interfaces.Services;

/// <summary>
/// Relatórios da cantina (D5). Sem datas: últimos 30 dias. Pedidos cancelados não contam.
/// </summary>
public interface IRelatorioService
{
    Task<List<VendaDiaDto>> VendasPorDia(DateOnly? inicio, DateOnly? fim);
    Task<List<ItemVendidoDto>> RankingDeItens(DateOnly? inicio, DateOnly? fim);

    // Os dois relatórios juntos, com o período já resolvido
    Task<RelatorioVendasDto> Completo(DateOnly? inicio, DateOnly? fim);
}
