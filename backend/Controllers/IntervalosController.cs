using Backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/intervalos")]
public class IntervalosController : ControllerBase
{
    private readonly AppDbContext _db;

    public IntervalosController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Intervalos do dia (Manhã, Tarde) com horários. O front usa para o seletor e o aviso "fecha às 08:45".
    /// </summary>
    [Authorize]
    [HttpGet]
    public async Task<object> Listar()
    {
        return await _db.Intervalos
            .OrderBy(i => i.HoraInicio)
            .Select(i => new
            {
                i.Id,
                i.Nome,
                i.HoraInicio,
                i.HoraFim,
                Fechamento = i.HoraInicio.AddMinutes(-i.MinutosAntecedencia),
            })
            .ToListAsync();
    }
}
