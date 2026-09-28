using Backend.Domain.Models;

namespace Backend.Domain.Interfaces.Repositories;

public interface IMetodoPagamentoRepository
{
    // Método ativo que pertence ao responsável
    Task<MetodoPagamento?> BuscarAtivo(int id, int adultoId);

    void Adicionar(MetodoPagamento metodo);
}
