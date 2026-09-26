using Backend.Data;
using Backend.DTOs.Extratos;
using Backend.DTOs.Pedidos;
using Backend.Extensions;
using Backend.Models;
using Backend.Models.Enums;
using Backend.Reports;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

// Extrato do aluno (R7, cena 4). Veem: o próprio aluno, o responsável e o Admin.
[ApiController]
[Route("api/extratos")]
[Authorize]
public class ExtratosController : ControllerBase
{
    private readonly AppDbContext _db;

    public ExtratosController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Movimentos do mês, item a item. mes no formato "2026-08"; sem mes = mês atual.
    /// </summary>
    [HttpGet("aluno/{id}")]
    public async Task<ActionResult<ExtratoDto>> Extrato(int id, string? mes)
    {
        var aluno = await _db.BuscarAlunoVisivel(User, id);
        if (aluno == null)
            return NotFound();

        return await MontarExtrato(aluno, mes);
    }

    /// <summary>
    /// O mesmo extrato em PDF, para baixar.
    /// </summary>
    [HttpGet("aluno/{id}/pdf")]
    public async Task<IActionResult> ExtratoPdf(int id, string? mes)
    {
        var aluno = await _db.BuscarAlunoVisivel(User, id);
        if (aluno == null)
            return NotFound();

        var extrato = await MontarExtrato(aluno, mes);
        var pdf = PdfExportService.Extrato(extrato);

        return File(pdf, "application/pdf", $"extrato-{extrato.Mes:yyyy-MM}.pdf");
    }

    private async Task<ExtratoDto> MontarExtrato(Aluno aluno, string? mes)
    {
        var inicio = PrimeiroDiaDoMes(mes);
        var inicioDoMes = inicio.ToDateTime(TimeOnly.MinValue);
        var inicioDoProximo = inicioDoMes.AddMonths(1);

        var movimentos = await _db.Movimentos
            .AsNoTracking()
            .Include(m => m.Pedido).ThenInclude(p => p!.Itens).ThenInclude(ip => ip.Item)
            .Where(m => m.Conta.UsuarioId == aluno.UsuarioId &&
                        m.Data >= inicioDoMes &&
                        m.Data < inicioDoProximo)
            .OrderBy(m => m.Data)
            .ToListAsync();

        return new ExtratoDto
        {
            AlunoId = aluno.UsuarioId,
            AlunoNome = aluno.Usuario.Nome,
            Mes = inicio,
            SaldoAtual = aluno.Usuario.Conta?.Saldo ?? 0,
            TotalGasto = -movimentos.Where(m => m.Tipo is TipoMovimento.Compra or TipoMovimento.Estorno).Sum(m => m.Valor),
            TotalCreditos = movimentos.Where(m => m.Tipo is TipoMovimento.Credito or TipoMovimento.Pagamento).Sum(m => m.Valor),
            Movimentos = movimentos.Select(m => new MovimentoDto
            {
                Data = m.Data,
                Tipo = m.Tipo,
                Valor = m.Valor,
                Descricao = m.Descricao,
                SaldoApos = m.SaldoApos,
                Itens = m.Pedido?.Itens.Select(ip => new ItemPedidoDto
                {
                    ItemId = ip.ItemId,
                    Nome = ip.Item.Nome,
                    Quantidade = ip.Quantidade,
                    PrecoUnitario = ip.PrecoUnitario,
                    Subtotal = ip.Subtotal,
                }).ToList() ?? [],
            }).ToList(),
        };
    }

    // "2026-08" → 01/08/2026; vazio → primeiro dia do mês atual
    private static DateOnly PrimeiroDiaDoMes(string? mes)
    {
        if (string.IsNullOrEmpty(mes))
        {
            var hoje = DateOnly.FromDateTime(DateTime.Now);
            return new DateOnly(hoje.Year, hoje.Month, 1);
        }

        if (!DateOnly.TryParseExact(mes + "-01", "yyyy-MM-dd", out var inicio))
            throw new RegraException("Mês inválido. Use o formato 2026-08");

        return inicio;
    }
}
