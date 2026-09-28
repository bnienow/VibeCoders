using Backend.Domain.DTOs.Itens;
using Backend.Domain.Helpers;
using Backend.Domain.Interfaces.Repositories;
using Backend.Domain.Interfaces.Services;
using Backend.Domain.Models;

namespace Backend.Data.Services;

public class ItemService : IItemService
{
    private readonly IItemRepository _itens;
    private readonly IAlunoRepository _alunos;
    private readonly IUnitOfWork _uow;

    public ItemService(IItemRepository itens, IAlunoRepository alunos, IUnitOfWork uow)
    {
        _itens = itens;
        _alunos = alunos;
        _uow = uow;
    }

    public async Task<List<ItemDto>> Cardapio(DateOnly? data, int? intervaloId, int usuarioId)
    {
        var itens = await _itens.Cardapio(data ?? DateOnly.FromDateTime(DateTime.Now), intervaloId);

        // Adulto e Admin não são alunos, então não têm restrições
        var restricoes = (await _alunos.BuscarPorId(usuarioId))?.RestricoesAlimentares ?? "";

        return itens.Select(i => ParaDto(i, restricoes)).ToList();
    }

    public async Task<List<ItemDto>> Listar() =>
        (await _itens.Listar()).Select(i => ParaDto(i, "")).ToList();

    public async Task<ItemDto> Criar(SalvarItemDto dto)
    {
        var item = new Item();
        Preencher(item, dto);

        _itens.Adicionar(item);
        await _uow.SalvarAsync();

        return ParaDto(item, "");
    }

    /// <summary>
    /// Pedidos antigos não mudam: o preço fica congelado em ItemPedido.
    /// </summary>
    public async Task<ItemDto?> Editar(int id, SalvarItemDto dto)
    {
        var item = await _itens.BuscarPorId(id);
        if (item == null)
            return null;

        Preencher(item, dto);
        await _uow.SalvarAsync();

        return ParaDto(item, "");
    }

    /// <summary>
    /// Some do cardápio, mas o histórico de pedidos continua.
    /// </summary>
    public async Task<bool> Desativar(int id)
    {
        var item = await _itens.BuscarPorId(id);
        if (item == null)
            return false;

        item.Ativo = false;
        await _uow.SalvarAsync();

        return true;
    }

    public async Task DefinirDisponibilidade(DateOnly data, int intervaloId, int itemId, bool disponivel)
    {
        var disponibilidade = await _itens.BuscarDisponibilidade(data, intervaloId, itemId);

        if (disponibilidade == null)
        {
            disponibilidade = new DispCardapio { Data = data, IntervaloId = intervaloId, ItemId = itemId };
            _itens.AdicionarDisponibilidade(disponibilidade);
        }

        disponibilidade.Disponivel = disponivel;
        await _uow.SalvarAsync();
    }

    // Copia os campos do formulário para a entidade (usado no criar e no editar)
    private static void Preencher(Item item, SalvarItemDto dto)
    {
        item.Nome = dto.Nome;
        item.Descricao = dto.Descricao;
        item.PrecoUnitario = dto.PrecoUnitario;
        item.Estoque = dto.Estoque;
        item.Categoria = dto.Categoria;
        item.Alergenos = Alergia.ParaTexto(dto.Alergenos);
    }

    private static ItemDto ParaDto(Item item, string restricoes) => new()
    {
        Id = item.Id,
        Nome = item.Nome,
        Descricao = item.Descricao,
        PrecoUnitario = item.PrecoUnitario,
        Estoque = item.Estoque,
        Categoria = item.Categoria,
        Alergenos = Alergia.ParaLista(item.Alergenos),
        Ativo = item.Ativo,
        ConflitaComRestricao = Alergia.TemConflito(item.Alergenos, restricoes),
    };
}
