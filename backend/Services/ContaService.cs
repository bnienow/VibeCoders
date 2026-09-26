using Backend.Data;
using Backend.Models;
using Backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

/// <summary>
/// Conta corrente do usuário: todo dinheiro que entra ou sai passa por aqui e vira um Movimento (extrato).
/// Não chama SaveChanges: quem chama grava junto com o resto da operação, numa transação só.
/// </summary>
public class ContaService
{
    private readonly AppDbContext _db;

    public ContaService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Soma o valor no saldo e registra o Movimento com o saldo resultante.
    /// Valor negativo = saiu (compra); positivo = entrou (crédito, pagamento, estorno).
    /// </summary>
    public async Task Movimentar(int usuarioId, decimal valor, TipoMovimento tipo, string descricao, Pedido? pedido = null)
    {
        var conta = await _db.Contas.SingleOrDefaultAsync(c => c.UsuarioId == usuarioId)
            ?? throw new RegraException("Conta não encontrada");

        conta.Saldo += valor;
        conta.AtualizadaEm = DateTime.Now;

        _db.Movimentos.Add(new Movimento
        {
            Conta = conta,
            Tipo = tipo,
            Valor = valor,
            Data = DateTime.Now,
            Pedido = pedido,
            Descricao = descricao,
            SaldoApos = conta.Saldo,
        });
    }

    /// <summary>
    /// Responsável coloca crédito na própria conta ou na de um filho, com pagamento simulado (D1: sempre aprovado).
    /// </summary>
    public async Task AdicionarCredito(int adultoId, int destinoId, decimal valor, int? metodoPagamentoId)
    {
        var destinoValido = destinoId == adultoId ||
                            await _db.Alunos.AnyAsync(a => a.UsuarioId == destinoId && a.AdultoId == adultoId);
        if (!destinoValido)
            throw new RegraException("Só é possível colocar crédito na sua conta ou na de um filho");

        var metodo = metodoPagamentoId == null
            ? null
            : await _db.MetodosPagamento.FirstOrDefaultAsync(m => m.Id == metodoPagamentoId && m.AdultoId == adultoId && m.Ativo)
              ?? throw new RegraException("Método de pagamento não encontrado");

        await Movimentar(destinoId, valor, TipoMovimento.Credito, $"Crédito via {metodo?.Apelido ?? "pagamento simulado"}");
    }
}
