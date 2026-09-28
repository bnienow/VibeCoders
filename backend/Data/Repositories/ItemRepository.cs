using Backend.Data;
using Backend.Domain.Interfaces.Repositories;
using Backend.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Repositories;

public class ItemRepository : IItemRepository
{
    private readonly AppDbContext _db;

    public ItemRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<Item>> Cardapio(DateOnly dia, int? intervaloId)
    {
        var itens = _db.Itens.Where(i => i.Ativo && i.Estoque > 0);

        if (intervaloId != null)
            itens = itens.Where(i => _db.DispCardapios.Any(d =>
                d.ItemId == i.Id && d.Data == dia && d.IntervaloId == intervaloId && d.Disponivel));

        return itens.ToListAsync();
    }

    public Task<List<Item>> Listar() => _db.Itens.ToListAsync();

    public async Task<Item?> BuscarPorId(int id) => await _db.Itens.FindAsync(id);

    public Task<Dictionary<int, Item>> BuscarPorIds(List<int> ids) =>
        _db.Itens
            .Where(i => ids.Contains(i.Id))
            .ToDictionaryAsync(i => i.Id);

    public Task<bool> OferecidoNoIntervalo(int itemId, DateOnly data, int intervaloId) =>
        _db.DispCardapios.AnyAsync(d =>
            d.ItemId == itemId && d.Data == data && d.IntervaloId == intervaloId && d.Disponivel);

    public async Task<DispCardapio?> BuscarDisponibilidade(DateOnly data, int intervaloId, int itemId) =>
        await _db.DispCardapios.FindAsync(data, intervaloId, itemId);

    public void Adicionar(Item item) => _db.Itens.Add(item);

    public void AdicionarDisponibilidade(DispCardapio disponibilidade) => _db.DispCardapios.Add(disponibilidade);
}
