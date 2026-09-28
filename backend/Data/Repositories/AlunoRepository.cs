using Backend.Data;
using Backend.Domain.Interfaces.Repositories;
using Backend.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Repositories;

public class AlunoRepository : IAlunoRepository
{
    private readonly AppDbContext _db;

    public AlunoRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Aluno?> BuscarComConta(int id) =>
        ComConta().FirstOrDefaultAsync(a => a.UsuarioId == id);

    public Task<Aluno?> BuscarFilho(int id, int adultoId) =>
        ComConta().FirstOrDefaultAsync(a => a.UsuarioId == id && a.AdultoId == adultoId);

    public Task<List<Aluno>> Buscar(string? busca, int limite) =>
        ComConta()
            .Where(a => a.Usuario.Ativo &&
                        (busca == null || a.Usuario.Nome.Contains(busca) || a.Usuario.Email.Contains(busca)))
            .OrderBy(a => a.Usuario.Nome)
            .Take(limite)
            .ToListAsync();

    public Task<List<Aluno>> FilhosDe(int adultoId) =>
        ComConta()
            .Where(a => a.AdultoId == adultoId)
            .OrderBy(a => a.Usuario.Nome)
            .ToListAsync();

    public async Task<Aluno?> BuscarPorId(int id) =>
        await _db.Alunos.FindAsync(id);

    public Task<bool> EhFilho(int alunoId, int adultoId) =>
        _db.Alunos.AnyAsync(a => a.UsuarioId == alunoId && a.AdultoId == adultoId);

    public void Adicionar(Aluno aluno) => _db.Alunos.Add(aluno);

    private IQueryable<Aluno> ComConta() =>
        _db.Alunos.Include(a => a.Usuario).ThenInclude(u => u.Conta);
}
