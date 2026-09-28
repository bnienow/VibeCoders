using Backend.Domain.Models;

namespace Backend.Domain.Interfaces.Repositories;

public interface IFechamentoRepository
{
    // Do mais recente para o mais antigo
    Task<List<Fechamento>> DoAdulto(int adultoId);

    Task<Fechamento?> BuscarDoAdulto(int id, int adultoId);

    void Adicionar(Fechamento fechamento);
}
