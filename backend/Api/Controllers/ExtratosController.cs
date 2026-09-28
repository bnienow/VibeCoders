using Backend.Api.Extensions;
using Backend.Api.Reports;
using Backend.Domain.DTOs.Extratos;
using Backend.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

// Extrato do aluno (R7, cena 4). Veem: o próprio aluno, o responsável e o Admin.
[ApiController]
[Route("api/extratos")]
[Authorize]
public class ExtratosController : ControllerBase
{
    private readonly IExtratoService _extratos;

    public ExtratosController(IExtratoService extratos)
    {
        _extratos = extratos;
    }

    /// <summary>
    /// Movimentos do mês, item a item. mes no formato "2026-08"; sem mes = mês atual.
    /// </summary>
    [HttpGet("aluno/{id}")]
    public async Task<ActionResult<ExtratoDto>> Extrato(int id, string? mes)
    {
        var extrato = await _extratos.DoAluno(id, mes, User.UsuarioId(), User.IsInRole("Admin"));
        if (extrato == null)
            return NotFound();

        return extrato;
    }

    /// <summary>
    /// O mesmo extrato em PDF, para baixar.
    /// </summary>
    [HttpGet("aluno/{id}/pdf")]
    public async Task<IActionResult> ExtratoPdf(int id, string? mes)
    {
        var extrato = await _extratos.DoAluno(id, mes, User.UsuarioId(), User.IsInRole("Admin"));
        if (extrato == null)
            return NotFound();

        return File(PdfExportService.Extrato(extrato), "application/pdf", $"extrato-{extrato.Mes:yyyy-MM}.pdf");
    }
}
