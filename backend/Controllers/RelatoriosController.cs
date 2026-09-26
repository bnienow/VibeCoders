using Backend.Data;
using Backend.DTOs.Relatorios;
using Backend.Models.Enums;
using Backend.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

// Relatórios da cantina (D5). Período padrão: últimos 30 dias. Pedidos cancelados não contam.
[ApiController]
[Route("api/relatorios")]
[Authorize(Roles = "Admin")]
public class RelatoriosController : ControllerBase
{
    private readonly AppDbContext _db;

    public RelatoriosController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Quantidade de pedidos e total vendido por dia.
    /// </summary>
    [HttpGet("vendas")]
    public Task<List<VendaDiaDto>> Vendas(DateOnly? inicio, DateOnly? fim) =>
        VendasPorDia(inicio, fim);

    /// <summary>
    /// Ranking dos itens mais vendidos no período.
    /// </summary>
    [HttpGet("itens-mais-vendidos")]
    public Task<List<ItemVendidoDto>> ItensMaisVendidos(DateOnly? inicio, DateOnly? fim) =>
        RankingDeItens(inicio, fim);

    /// <summary>
    /// Os dois relatórios numa planilha Excel.
    /// </summary>
    [HttpGet("vendas/excel")]
    public async Task<IActionResult> VendasExcel(DateOnly? inicio, DateOnly? fim)
    {
        var planilha = ExcelExportService.Vendas(await VendasPorDia(inicio, fim), await RankingDeItens(inicio, fim));

        return File(planilha, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "vendas.xlsx");
    }

    private async Task<List<VendaDiaDto>> VendasPorDia(DateOnly? inicio, DateOnly? fim)
    {
        var (de, ate) = Periodo(inicio, fim);

        return await _db.Pedidos
            .Where(p => p.Data >= de && p.Data <= ate && p.Status != StatusPedido.Cancelado)
            .GroupBy(p => p.Data)
            .Select(g => new VendaDiaDto { Data = g.Key, Pedidos = g.Count(), Total = g.Sum(p => p.Total) })
            .OrderBy(v => v.Data)
            .ToListAsync();
    }

    private async Task<List<ItemVendidoDto>> RankingDeItens(DateOnly? inicio, DateOnly? fim)
    {
        var (de, ate) = Periodo(inicio, fim);

        return await _db.ItensPedido
            .Where(ip => ip.Pedido.Data >= de && ip.Pedido.Data <= ate && ip.Pedido.Status != StatusPedido.Cancelado)
            .GroupBy(ip => ip.Item.Nome)
            .Select(g => new ItemVendidoDto { Nome = g.Key, Quantidade = g.Sum(ip => ip.Quantidade), Total = g.Sum(ip => ip.Subtotal) })
            .OrderByDescending(i => i.Quantidade)
            .ToListAsync();
    }

    // Sem datas: dos últimos 30 dias até hoje
    private static (DateOnly De, DateOnly Ate) Periodo(DateOnly? inicio, DateOnly? fim)
    {
        var ate = fim ?? DateOnly.FromDateTime(DateTime.Now);

        return (inicio ?? ate.AddDays(-30), ate);
    }
}
