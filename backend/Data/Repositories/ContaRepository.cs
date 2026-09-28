using Backend.Data;
using Backend.Domain.Interfaces.Repositories;
using Backend.Domain.Models;
using Backend.Domain.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Repositories;

public class ContaRepository : IContaRepository
{
    private readonly AppDbContext _db;

    public ContaRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Conta?> BuscarPorUsuario(int usuarioId) =>
        _db.Contas.SingleOrDefaultAsync(c => c.UsuarioId == usuarioId);

    public Task<bool> Existe(int usuarioId) =>
        _db.Contas.AnyAsync(c => c.UsuarioId == usuarioId);

    public Task<decimal> FiadoDaFamilia(int adultoId) =>
        DaFamilia(adultoId)
            .Where(c => c.Saldo < 0)
            .SumAsync(c => -c.Saldo);

    public Task<List<Conta>> NegativasDaFamilia(int adultoId) =>
        DaFamilia(adultoId)
            .Where(c => c.Saldo < 0)
            .OrderBy(c => c.Saldo)
            .ToListAsync();

    // Compras são negativas e estornos positivos: a soma invertida é o gasto líquido
    public Task<Dictionary<int, decimal>> GastosDesde(List<int> usuarioIds, DateTime inicio) =>
        _db.Movimentos
            .Where(m => usuarioIds.Contains(m.Conta.UsuarioId) &&
                        m.Data >= inicio &&
                        (m.Tipo == TipoMovimento.Compra || m.Tipo == TipoMovimento.Estorno))
            .GroupBy(m => m.Conta.UsuarioId)
            .Select(g => new { UsuarioId = g.Key, Gasto = -g.Sum(m => m.Valor) })
            .ToDictionaryAsync(g => g.UsuarioId, g => g.Gasto);

    public Task<List<Movimento>> MovimentosDoPeriodo(int usuarioId, DateTime inicio, DateTime fim) =>
        _db.Movimentos
            .AsNoTracking()
            .Include(m => m.Pedido).ThenInclude(p => p!.Itens).ThenInclude(ip => ip.Item)
            .Where(m => m.Conta.UsuarioId == usuarioId && m.Data >= inicio && m.Data < fim)
            .OrderBy(m => m.Data)
            .ToListAsync();

    public void Adicionar(Conta conta) => _db.Contas.Add(conta);

    public void AdicionarMovimento(Movimento movimento) => _db.Movimentos.Add(movimento);

    // Conta do responsável e dos filhos dele
    private IQueryable<Conta> DaFamilia(int adultoId) =>
        _db.Contas.Where(c =>
            c.UsuarioId == adultoId ||
            _db.Alunos.Any(a => a.UsuarioId == c.UsuarioId && a.AdultoId == adultoId));
}
