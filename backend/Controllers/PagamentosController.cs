using Backend.Data;
using Backend.DTOs.Pagamentos;
using Backend.Extensions;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

// Métodos de pagamento e crédito simulado (D1). Nada aqui fala com gateway real: todo pagamento é aprovado.
[ApiController]
[Route("api/pagamentos")]
[Authorize(Roles = "Adulto")]
public class PagamentosController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ContaService _conta;

    public PagamentosController(AppDbContext db, ContaService conta)
    {
        _db = db;
        _conta = conta;
    }

    /// <summary>
    /// Métodos ativos do responsável logado, indicando o padrão.
    /// </summary>
    [HttpGet("metodos")]
    public async Task<List<MetodoPagamentoDto>> Metodos()
    {
        var adulto = await _db.Adultos
            .Include(a => a.MetodosPagamento)
            .SingleAsync(a => a.UsuarioId == User.UsuarioId());

        return adulto.MetodosPagamento
            .Where(m => m.Ativo)
            .Select(m => ParaDto(m, adulto))
            .ToList();
    }

    /// <summary>
    /// Cadastra um Pix ou cartão fictício. O primeiro cadastrado vira padrão automaticamente.
    /// </summary>
    [HttpPost("metodos")]
    public async Task<MetodoPagamentoDto> Cadastrar(CreateMetodoPagamentoDto dto)
    {
        var adulto = await _db.Adultos.SingleAsync(a => a.UsuarioId == User.UsuarioId());

        var metodo = new MetodoPagamento
        {
            Adulto = adulto,
            Tipo = dto.Tipo,
            Apelido = dto.Apelido,
            UltimosDigitos = dto.UltimosDigitos,
        };
        _db.MetodosPagamento.Add(metodo);

        if (dto.Padrao || adulto.MetodoPagamentoPadraoId == null)
            adulto.MetodoPagamentoPadrao = metodo;

        await _db.SaveChangesAsync();

        return ParaDto(metodo, adulto);
    }

    /// <summary>
    /// Simula um Pix ou cartão e credita na conta do responsável ou de um filho.
    /// </summary>
    [HttpPost("simular")]
    public async Task<IActionResult> Simular(SimularPagamentoDto dto)
    {
        await _conta.AdicionarCredito(User.UsuarioId(), dto.UsuarioId, dto.Valor, dto.MetodoPagamentoId);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static MetodoPagamentoDto ParaDto(MetodoPagamento m, Adulto adulto) => new()
    {
        Id = m.Id,
        Tipo = m.Tipo,
        Apelido = m.Apelido,
        UltimosDigitos = m.UltimosDigitos,
        Padrao = adulto.MetodoPagamentoPadraoId == m.Id,
    };
}
