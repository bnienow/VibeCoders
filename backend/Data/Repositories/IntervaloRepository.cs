using Backend.Data;
using Backend.Domain.Interfaces.Repositories;
using Backend.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Repositories;

public class IntervaloRepository : IIntervaloRepository
{
    private readonly AppDbContext _db;

    public IntervaloRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<Intervalo>> Listar() =>
        _db.Intervalos.OrderBy(i => i.HoraInicio).ToListAsync();

    public async Task<Intervalo?> BuscarPorId(int id) =>
        await _db.Intervalos.FindAsync(id);

    public Task<Intervalo?> EmAndamento(TimeOnly hora) =>
        _db.Intervalos.FirstOrDefaultAsync(i => i.HoraInicio <= hora && hora <= i.HoraFim);

    public Task<Intervalo?> Proximo(TimeOnly hora) =>
        _db.Intervalos
            .Where(i => i.HoraFim >= hora)
            .OrderBy(i => i.HoraInicio)
            .FirstOrDefaultAsync();
}
