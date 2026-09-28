using Backend.Api.Extensions;
using Backend.Domain.DTOs.Itens;
using Backend.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/itens")]
public class ItensController : ControllerBase
{
    private readonly IItemService _itens;

    public ItensController(IItemService itens)
    {
        _itens = itens;
    }

    /// <summary>
    /// Cardápio: itens ativos e com estoque.
    /// Com intervaloId, só os oferecidos naquele dia e intervalo (DispCardapio).
    /// Sem intervaloId, é o balcão fora de intervalo.
    /// </summary>
    [Authorize]
    [HttpGet("cardapio")]
    public Task<List<ItemDto>> Cardapio(DateOnly? data, int? intervaloId) =>
        _itens.Cardapio(data, intervaloId, User.UsuarioId());

    /// <summary>
    /// Todos os itens, inclusive inativos e esgotados (gestão do Admin).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public Task<List<ItemDto>> Listar() => _itens.Listar();

    /// <summary>
    /// Cria um item.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public Task<ItemDto> Criar(SalvarItemDto dto) => _itens.Criar(dto);

    /// <summary>
    /// Edita um item, inclusive preço e estoque (edição inline da tabela).
    /// Pedidos antigos não mudam: o preço fica congelado em ItemPedido.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<ItemDto>> Editar(int id, SalvarItemDto dto)
    {
        var item = await _itens.Editar(id, dto);
        if (item == null)
            return NotFound();

        return item;
    }

    /// <summary>
    /// Desativa o item: some do cardápio, mas o histórico de pedidos continua.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Desativar(int id) =>
        await _itens.Desativar(id) ? NoContent() : NotFound();

    /// <summary>
    /// Liga ou desliga um item no cardápio de um dia e intervalo.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("/api/cardapio/{data}/{intervaloId}/{itemId}")]
    public async Task<IActionResult> DefinirDisponibilidade(DateOnly data, int intervaloId, int itemId, DisponibilidadeDto dto)
    {
        await _itens.DefinirDisponibilidade(data, intervaloId, itemId, dto.Disponivel);

        return NoContent();
    }
}
