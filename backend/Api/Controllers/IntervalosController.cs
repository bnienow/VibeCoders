using Backend.Domain.DTOs.Intervalos;
using Backend.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/intervalos")]
public class IntervalosController : ControllerBase
{
    private readonly IIntervaloService _intervalos;

    public IntervalosController(IIntervaloService intervalos)
    {
        _intervalos = intervalos;
    }

    /// <summary>
    /// Intervalos do dia (Manhã, Tarde) com horários. O front usa para o seletor e o aviso "fecha às 08:45".
    /// </summary>
    [Authorize]
    [HttpGet]
    public Task<List<IntervaloDto>> Listar() => _intervalos.Listar();
}
