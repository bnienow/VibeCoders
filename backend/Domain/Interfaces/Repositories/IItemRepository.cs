using Backend.Domain.Models;

namespace Backend.Domain.Interfaces.Repositories;

public interface IItemRepository
{
    // Ativos e com estoque; com intervaloId, só os oferecidos naquele dia e intervalo
    Task<List<Item>> Cardapio(DateOnly dia, int? intervaloId);

    Task<List<Item>> Listar();
    Task<Item?> BuscarPorId(int id);
    Task<Dictionary<int, Item>> BuscarPorIds(List<int> ids);

    Task<bool> OferecidoNoIntervalo(int itemId, DateOnly data, int intervaloId);
    Task<DispCardapio?> BuscarDisponibilidade(DateOnly data, int intervaloId, int itemId);

    void Adicionar(Item item);
    void AdicionarDisponibilidade(DispCardapio disponibilidade);
}
