using Backend.Data;
using Backend.Domain.DTOs.Relatorios;
using Backend.Domain.Interfaces.Repositories;
using Backend.Domain.Models;
using Backend.Domain.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Repositories;

public class PedidoRepository : IPedidoRepository
{
    private readonly AppDbContext _db;

    public PedidoRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Pedido?> BuscarComDetalhes(int id) =>
        ComDetalhes().FirstOrDefaultAsync(p => p.Id == id);

    public Task<List<Pedido>> DoUsuario(int usuarioId) =>
        ComDetalhes()
            .Where(p => p.UsuarioId == usuarioId)
            .OrderByDescending(p => p.CriadoEm)
            .ToListAsync();

    public Task<List<Pedido>> AntecipadosDoIntervalo(DateOnly dia, int intervaloId) =>
        ComDetalhes()
            .Where(p => p.Data == dia &&
                        p.IntervaloId == intervaloId &&
                        p.TipoVenda == TipoVenda.Antecipado &&
                        p.Status != StatusPedido.Cancelado)
            .OrderBy(p => p.CriadoEm)
            .ToListAsync();

    public Task<List<Pedido>> BuscarAntecipados(DateOnly dia, string termo) =>
        ComDetalhes()
            .Where(p => p.Data == dia &&
                        p.TipoVenda == TipoVenda.Antecipado &&
                        p.Status != StatusPedido.Cancelado &&
                        (p.CodigoRetirada == termo ||
                         p.Usuario.Nome.Contains(termo) ||
                         p.Usuario.Email.Contains(termo)))
            .ToListAsync();

    public Task<Pedido?> BuscarParaEditar(int id, int usuarioId) =>
        _db.Pedidos
            .Include(p => p.Intervalo)
            .Include(p => p.Itens).ThenInclude(ip => ip.Item)
            .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == usuarioId);

    public async Task<Pedido?> BuscarPorId(int id) =>
        await _db.Pedidos.FindAsync(id);

    public Task<bool> TemAntecipado(int usuarioId, DateOnly data, int intervaloId) =>
        _db.Pedidos.AnyAsync(p =>
            p.UsuarioId == usuarioId &&
            p.Data == data &&
            p.IntervaloId == intervaloId &&
            p.TipoVenda == TipoVenda.Antecipado &&
            p.Status != StatusPedido.Cancelado);

    public Task<decimal> GastoNaContaDoDia(int usuarioId, DateOnly data, int? ignorarPedidoId) =>
        _db.Pedidos
            .Where(p => p.UsuarioId == usuarioId &&
                        p.Data == data &&
                        p.Id != ignorarPedidoId &&
                        p.FormaPagamento == FormaPagamento.Conta &&
                        p.Status != StatusPedido.Cancelado)
            .SumAsync(p => p.Total);

    public Task<bool> CodigoEmUso(DateOnly data, string codigo) =>
        _db.Pedidos.AnyAsync(p => p.Data == data && p.CodigoRetirada == codigo);

    public Task<List<ConsumoLinha>> ConsumoDaFamilia(int adultoId, DateOnly inicio, DateOnly fim) =>
        _db.ItensPedido
            .Where(ip => ip.Pedido.Data >= inicio &&
                         ip.Pedido.Data < fim &&
                         ip.Pedido.FormaPagamento == FormaPagamento.Conta &&
                         ip.Pedido.Status != StatusPedido.Cancelado &&
                         (ip.Pedido.UsuarioId == adultoId ||
                          _db.Alunos.Any(a => a.UsuarioId == ip.Pedido.UsuarioId && a.AdultoId == adultoId)))
            .Select(ip => new ConsumoLinha(ip.Pedido.Usuario.Nome, ip.Item.Nome, ip.Quantidade, ip.Subtotal))
            .ToListAsync();

    public Task<List<VendaDiaDto>> VendasPorDia(DateOnly de, DateOnly ate) =>
        _db.Pedidos
            .Where(p => p.Data >= de && p.Data <= ate && p.Status != StatusPedido.Cancelado)
            .GroupBy(p => p.Data)
            .Select(g => new VendaDiaDto { Data = g.Key, Pedidos = g.Count(), Total = g.Sum(p => p.Total) })
            .OrderBy(v => v.Data)
            .ToListAsync();

    public Task<List<ItemVendidoDto>> RankingDeItens(DateOnly de, DateOnly ate) =>
        _db.ItensPedido
            .Where(ip => ip.Pedido.Data >= de && ip.Pedido.Data <= ate && ip.Pedido.Status != StatusPedido.Cancelado)
            .GroupBy(ip => ip.Item.Nome)
            .Select(g => new ItemVendidoDto { Nome = g.Key, Quantidade = g.Sum(ip => ip.Quantidade), Total = g.Sum(ip => ip.Subtotal) })
            .OrderByDescending(i => i.Quantidade)
            .ToListAsync();

    public void Adicionar(Pedido pedido) => _db.Pedidos.Add(pedido);

    private IQueryable<Pedido> ComDetalhes() =>
        _db.Pedidos
            .AsNoTracking()
            .Include(p => p.Usuario)
            .Include(p => p.Intervalo)
            .Include(p => p.Itens).ThenInclude(ip => ip.Item);
}
