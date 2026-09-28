using Backend.Api.Extensions;
using Backend.Domain.DTOs.Fechamentos;
using Backend.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

// Fechamento mensal (R7). As regras de valor e pagamento estão no FechamentoService.
[ApiController]
[Route("api/fechamentos")]
public class FechamentosController : ControllerBase
{
    private readonly IFechamentoService _fechamentos;

    public FechamentosController(IFechamentoService fechamentos)
    {
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
        var fechamentos = await _fechamentos.DoAdulto(adultoId, User.UsuarioId(), User.IsInRole("Admin"));
        if (fechamentos == null)
            return NotFound();

        return fechamentos;
    }

    /// <summary>
    /// Gera o fechamento do mês anterior para todos os responsáveis (rodar no dia 1º).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("gerar")]
    public async Task<object> Gerar() => new { Gerados = await _fechamentos.GerarMesAnterior() };

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
}
