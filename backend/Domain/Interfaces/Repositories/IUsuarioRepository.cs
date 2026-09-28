using Backend.Domain.Models;

namespace Backend.Domain.Interfaces.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> BuscarAtivoPorEmail(string email);
    Task<Usuario?> BuscarPorId(int id);
    Task<bool> EmailEmUso(string email);
}
