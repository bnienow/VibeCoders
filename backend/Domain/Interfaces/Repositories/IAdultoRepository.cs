using Backend.Domain.Models;

namespace Backend.Domain.Interfaces.Repositories;

public interface IAdultoRepository
{
    Task<Adulto> Buscar(int id);
    Task<Adulto> BuscarComMetodos(int id);

    // Responsáveis que ainda não têm fechamento no mês
    Task<List<int>> IdsSemFechamento(DateOnly mes);

    void Adicionar(Adulto adulto);
}
