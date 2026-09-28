using Backend.Api.Reports;
using Backend.Domain.DTOs.Relatorios;
using Backend.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

// Relatórios da cantina (D5). Período padrão: últimos 30 dias. Pedidos cancelados não contam.
[ApiController]
[Route("api/relatorios")]
[Authorize(Roles = "Admin")]
public class RelatoriosController : ControllerBase
{
    private readonly IRelatorioService _relatorios;

    public RelatoriosController(IRelatorioService relatorios)
    {
        _relatorios = relatorios;
    }

    /// <summary>
    /// Quantidade de pedidos e total vendido por dia.
    /// </summary>
    [HttpGet("vendas")]
    public Task<List<VendaDiaDto>> Vendas(DateOnly? inicio, DateOnly? fim) =>
        _relatorios.VendasPorDia(inicio, fim);

    /// <summary>
    /// Ranking dos itens mais vendidos no período.
    /// </summary>
    [HttpGet("itens-mais-vendidos")]
    public Task<List<ItemVendidoDto>> ItensMaisVendidos(DateOnly? inicio, DateOnly? fim) =>
        _relatorios.RankingDeItens(inicio, fim);

    /// <summary>
    /// Os dois relatórios numa planilha Excel.
    /// </summary>
    [HttpGet("vendas/excel")]
    public async Task<IActionResult> VendasExcel(DateOnly? inicio, DateOnly? fim)
    {
        var r = await _relatorios.Completo(inicio, fim);
        var planilha = ExcelExportService.Vendas(r.De, r.Ate, r.Vendas, r.Itens);

        return File(planilha, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"vendas-{r.De:yyyy-MM-dd}-a-{r.Ate:yyyy-MM-dd}.xlsx");
    }
}
