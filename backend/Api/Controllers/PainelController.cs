using Backend.Domain.DTOs.Painel;
using Backend.Domain.DTOs.Pedidos;
using Backend.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

// Painel da cantina (cena 2). Tudo aqui é só do Admin.
[ApiController]
[Route("api/painel")]
[Authorize(Roles = "Admin")]
public class PainelController : ControllerBase
{
    private readonly IPainelService _painel;

    public PainelController(IPainelService painel)
    {
        _painel = painel;
    }

    /// <summary>
    /// Pedidos antecipados do próximo intervalo de hoje (o que está acontecendo ou o próximo a começar).
    /// </summary>
    [HttpGet("intervalo-atual")]
    public Task<PainelDto> IntervaloAtual() => _painel.IntervaloAtual();

    /// <summary>
    /// Consolidado por item dos pedidos ainda não entregues (ex.: "12x Pão de queijo") para a cozinha.
    /// </summary>
    [HttpGet("preparo")]
    public Task<List<PreparoItemDto>> Preparo() => _painel.Preparo();

    /// <summary>
    /// Procura entre os pedidos antecipados de hoje por código de retirada, nome ou e-mail (R4).
    /// </summary>
    [HttpGet("buscar")]
    public Task<List<PedidoDto>> Buscar(string termo) => _painel.Buscar(termo);
}
