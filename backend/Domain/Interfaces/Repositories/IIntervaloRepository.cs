using Backend.Domain.Models;

namespace Backend.Domain.Interfaces.Repositories;

public interface IIntervaloRepository
{
    // Ordenados pela hora de início
    Task<List<Intervalo>> Listar();

    Task<Intervalo?> BuscarPorId(int id);

    // Intervalo em andamento nessa hora, se houver
    Task<Intervalo?> EmAndamento(TimeOnly hora);

    // Primeiro intervalo que ainda não terminou nessa hora
    Task<Intervalo?> Proximo(TimeOnly hora);
}
