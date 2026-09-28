using Backend.Data;
using Backend.Domain.Interfaces.Repositories;
using Backend.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _db;

    public UsuarioRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Usuario?> BuscarAtivoPorEmail(string email) =>
        _db.Usuarios.SingleOrDefaultAsync(u => u.Email == email && u.Ativo);

    public async Task<Usuario?> BuscarPorId(int id) =>
        await _db.Usuarios.FindAsync(id);

    public Task<bool> EmailEmUso(string email) =>
        _db.Usuarios.AnyAsync(u => u.Email == email);
}
