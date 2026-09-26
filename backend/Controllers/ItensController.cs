using Backend.Data;
using Backend.DTOs.Itens;
using Backend.Extensions;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/itens")]
public class ItensController : ControllerBase
{
    private readonly AppDbContext _db;

    public ItensController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Cardápio: itens ativos e com estoque.
    /// Com intervaloId, só os oferecidos naquele dia e intervalo (DispCardapio).
    /// Sem intervaloId, é o balcão fora de intervalo.
    /// </summary>
    [Authorize]
    [HttpGet("cardapio")]
    public async Task<List<ItemDto>> Cardapio(DateOnly? data, int? intervaloId)
    {
        var dia = data ?? DateOnly.FromDateTime(DateTime.Now);

        var itens = _db.Itens.Where(i => i.Ativo && i.Estoque > 0);

        if (intervaloId != null)
            itens = itens.Where(i => _db.DispCardapios.Any(d =>
                d.ItemId == i.Id && d.Data == dia && d.IntervaloId == intervaloId && d.Disponivel));

        // Restrições do aluno logado; adulto e Admin não têm linha em Alunos, então fica vazio
        var restricoes = await _db.Alunos
            .Where(a => a.UsuarioId == User.UsuarioId())
            .Select(a => a.RestricoesAlimentares)
            .FirstOrDefaultAsync() ?? "";

        return await itens.Select(i => ParaDto(i, restricoes)).ToListAsync();
    }

    /// <summary>
    /// Todos os itens, inclusive inativos e esgotados (gestão do Admin).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<List<ItemDto>> Listar()
    {
        return await _db.Itens.Select(i => ParaDto(i, "")).ToListAsync();
    }

    /// <summary>
    /// Cria um item.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ItemDto> Criar(SalvarItemDto dto)
    {
        var item = new Item();
        Preencher(item, dto);

        _db.Itens.Add(item);
        await _db.SaveChangesAsync();

        return ParaDto(item, "");
    }

    /// <summary>
    /// Edita um item, inclusive preço e estoque (edição inline da tabela).
    /// Pedidos antigos não mudam: o preço fica congelado em ItemPedido.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<ItemDto>> Editar(int id, SalvarItemDto dto)
    {
        var item = await _db.Itens.FindAsync(id);
        if (item == null)
            return NotFound();

        Preencher(item, dto);
        await _db.SaveChangesAsync();

        return ParaDto(item, "");
    }

    /// <summary>
    /// Desativa o item: some do cardápio, mas o histórico de pedidos continua.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Desativar(int id)
    {
        var item = await _db.Itens.FindAsync(id);
        if (item == null)
            return NotFound();

        item.Ativo = false;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Liga ou desliga um item no cardápio de um dia e intervalo.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("/api/cardapio/{data}/{intervaloId}/{itemId}")]
    public async Task<IActionResult> DefinirDisponibilidade(DateOnly data, int intervaloId, int itemId, DisponibilidadeDto dto)
    {
        var disponibilidade = await _db.DispCardapios.FindAsync(data, intervaloId, itemId);

        if (disponibilidade == null)
        {
            disponibilidade = new DispCardapio { Data = data, IntervaloId = intervaloId, ItemId = itemId };
            _db.DispCardapios.Add(disponibilidade);
        }

        disponibilidade.Disponivel = dto.Disponivel;
        await _db.SaveChangesAsync();

        return NoContent();
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

    // Converte a entidade no que a API devolve
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
