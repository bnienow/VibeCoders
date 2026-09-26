using Backend.Data;
using Backend.DTOs.Fechamentos;
using Backend.Extensions;
using Backend.Models.Enums;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

// Fechamento mensal (R7). As regras de valor e pagamento estão no FechamentoService.
[ApiController]
[Route("api/fechamentos")]
public class FechamentosController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly FechamentoService _fechamentos;

    public FechamentosController(AppDbContext db, FechamentoService fechamentos)
    {
        _db = db;
        _fechamentos = fechamentos;
    }

    /// <summary>
    /// Fechamentos do responsável, do mais recente para o mais antigo, com o consumo do mês item a item.
    /// Só o próprio responsável ou o Admin.
    /// </summary>
    [Authorize]
    [HttpGet("adulto/{adultoId}")]
    public async Task<ActionResult<List<FechamentoDto>>> DoAdulto(int adultoId)
    {
        if (adultoId != User.UsuarioId() && !User.IsInRole("Admin"))
            return NotFound();

        var fechamentos = await _db.Fechamentos
            .Where(f => f.AdultoId == adultoId)
            .OrderByDescending(f => f.MesReferencia)
            .ToListAsync();

        var resultado = new List<FechamentoDto>();
        foreach (var f in fechamentos)
        {
            resultado.Add(new FechamentoDto
            {
                Id = f.Id,
                MesReferencia = f.MesReferencia,
                ValorTotal = f.ValorTotal,
                Status = f.Status,
                GeradoEm = f.GeradoEm,
                PagoEm = f.PagoEm,
                Consumo = await ConsumoDoMes(adultoId, f.MesReferencia),
            });
        }

        return resultado;
    }

    /// <summary>
    /// Gera o fechamento do mês anterior para todos os responsáveis (rodar no dia 1º).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("gerar")]
    public async Task<object> Gerar()
    {
        var gerados = await _fechamentos.GerarMesAnterior();

        return new { Gerados = gerados };
    }

    /// <summary>
    /// O responsável paga o próprio fechamento (pagamento simulado, D1).
    /// </summary>
    [Authorize(Roles = "Adulto")]
    [HttpPost("{id}/pagar")]
    public async Task<IActionResult> Pagar(int id)
    {
        await _fechamentos.Pagar(id, User.UsuarioId());

        return NoContent();
    }

    // O que a família consumiu na conta no mês, por pessoa e item a item
    private async Task<List<ConsumoDto>> ConsumoDoMes(int adultoId, DateOnly mes)
    {
        var fimDoMes = mes.AddMonths(1);

        var itens = await _db.ItensPedido
            .Where(ip => ip.Pedido.Data >= mes &&
                         ip.Pedido.Data < fimDoMes &&
                         ip.Pedido.FormaPagamento == FormaPagamento.Conta &&
                         ip.Pedido.Status != StatusPedido.Cancelado &&
                         (ip.Pedido.UsuarioId == adultoId ||
                          _db.Alunos.Any(a => a.UsuarioId == ip.Pedido.UsuarioId && a.AdultoId == adultoId)))
            .Select(ip => new { Pessoa = ip.Pedido.Usuario.Nome, Item = ip.Item.Nome, ip.Quantidade, ip.Subtotal })
            .ToListAsync();

        return itens
            .GroupBy(i => i.Pessoa)
            .Select(pessoa => new ConsumoDto
            {
                Nome = pessoa.Key,
                Total = pessoa.Sum(i => i.Subtotal),
                Itens = pessoa
                    .GroupBy(i => i.Item)
                    .Select(item => new ConsumoItemDto
                    {
                        Nome = item.Key,
                        Quantidade = item.Sum(i => i.Quantidade),
                        Total = item.Sum(i => i.Subtotal),
                    })
                    .OrderByDescending(i => i.Total)
                    .ToList(),
            })
            .ToList();
    }
}
