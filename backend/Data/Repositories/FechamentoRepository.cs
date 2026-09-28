using Backend.Data;
using Backend.Domain.Interfaces.Repositories;
using Backend.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Repositories;

public class FechamentoRepository : IFechamentoRepository
{
    private readonly AppDbContext _db;

    public FechamentoRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<Fechamento>> DoAdulto(int adultoId) =>
        _db.Fechamentos
            .Where(f => f.AdultoId == adultoId)
            .OrderByDescending(f => f.MesReferencia)
            .ToListAsync();

    public Task<Fechamento?> BuscarDoAdulto(int id, int adultoId) =>
        _db.Fechamentos.FirstOrDefaultAsync(f => f.Id == id && f.AdultoId == adultoId);

    public void Adicionar(Fechamento fechamento) => _db.Fechamentos.Add(fechamento);
}
