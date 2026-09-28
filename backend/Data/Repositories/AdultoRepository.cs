using Backend.Data;
using Backend.Domain.Interfaces.Repositories;
using Backend.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Repositories;

public class AdultoRepository : IAdultoRepository
{
    private readonly AppDbContext _db;

    public AdultoRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Adulto> Buscar(int id) =>
        _db.Adultos.SingleAsync(a => a.UsuarioId == id);

    public Task<Adulto> BuscarComMetodos(int id) =>
        _db.Adultos
            .Include(a => a.MetodosPagamento)
            .SingleAsync(a => a.UsuarioId == id);

    public Task<List<int>> IdsSemFechamento(DateOnly mes) =>
        _db.Adultos
            .Where(a => !a.Fechamentos.Any(f => f.MesReferencia == mes))
            .Select(a => a.UsuarioId)
            .ToListAsync();

    public void Adicionar(Adulto adulto) => _db.Adultos.Add(adulto);
}
