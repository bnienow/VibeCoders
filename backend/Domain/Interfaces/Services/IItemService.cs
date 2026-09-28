using Backend.Domain.DTOs.Itens;

namespace Backend.Domain.Interfaces.Services;

public interface IItemService
{
    // Ativos e com estoque; com intervaloId, só os oferecidos naquele dia e intervalo.
    // Marca os itens que conflitam com as restrições do aluno logado
    Task<List<ItemDto>> Cardapio(DateOnly? data, int? intervaloId, int usuarioId);

    // Todos, inclusive inativos e esgotados (gestão do Admin)
    Task<List<ItemDto>> Listar();

    Task<ItemDto> Criar(SalvarItemDto dto);

    // Nulo se o item não existe
    Task<ItemDto?> Editar(int id, SalvarItemDto dto);

    // Falso se o item não existe
    Task<bool> Desativar(int id);

    Task DefinirDisponibilidade(DateOnly data, int intervaloId, int itemId, bool disponivel);
}
