using Backend.Data;
using Backend.Domain.Interfaces.Repositories;
using Backend.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Repositories;

public class MetodoPagamentoRepository : IMetodoPagamentoRepository
{
    private readonly AppDbContext _db;

    public MetodoPagamentoRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<MetodoPagamento?> BuscarAtivo(int id, int adultoId) =>
        _db.MetodosPagamento.FirstOrDefaultAsync(m => m.Id == id && m.AdultoId == adultoId && m.Ativo);

    public void Adicionar(MetodoPagamento metodo) => _db.MetodosPagamento.Add(metodo);
}
