using Backend.Domain.DTOs.Intervalos;
using Backend.Domain.Interfaces.Repositories;
using Backend.Domain.Interfaces.Services;

namespace Backend.Data.Services;

public class IntervaloService : IIntervaloService
{
    private readonly IIntervaloRepository _intervalos;

    public IntervaloService(IIntervaloRepository intervalos)
    {
        _intervalos = intervalos;
    }

    public async Task<List<IntervaloDto>> Listar() =>
        (await _intervalos.Listar())
            .Select(i => new IntervaloDto
            {
                Id = i.Id,
                Nome = i.Nome,
                HoraInicio = i.HoraInicio,
                HoraFim = i.HoraFim,
                Fechamento = i.HoraInicio.AddMinutes(-i.MinutosAntecedencia),
            })
            .ToList();
}
