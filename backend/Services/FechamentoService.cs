using Backend.Data;
using Backend.Models;
using Backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

/// <summary>
/// Fechamento mensal (R7). O valor a pagar é o fiado: a soma dos saldos negativos das contas da família
/// (responsável + filhos). O consumo do mês aparece só como informativo, para não cobrar duas vezes
/// o que já foi pago com crédito.
/// </summary>
public class FechamentoService
{
    private readonly AppDbContext _db;
    private readonly ContaService _conta;

    public FechamentoService(AppDbContext db, ContaService conta)
    {
        _db = db;
        _conta = conta;
    }

    /// <summary>
    /// Gera o fechamento do mês anterior para cada responsável que ainda não tem (dia 1º).
    /// Quem não tem fiado recebe o fechamento já como Pago.
    /// </summary>
    /// <returns>Quantos fechamentos foram criados.</returns>
    public async Task<int> GerarMesAnterior()
    {
        var hoje = DateOnly.FromDateTime(DateTime.Now);
        var mesAnterior = new DateOnly(hoje.Year, hoje.Month, 1).AddMonths(-1);
        var agora = DateTime.Now;

        var adultosSemFechamento = await _db.Adultos
            .Where(a => !a.Fechamentos.Any(f => f.MesReferencia == mesAnterior))
            .Select(a => a.UsuarioId)
            .ToListAsync();

        foreach (var adultoId in adultosSemFechamento)
        {
            var fiado = await ContasDaFamilia(adultoId)
                .Where(c => c.Saldo < 0)
                .SumAsync(c => -c.Saldo);

            _db.Fechamentos.Add(new Fechamento
            {
                AdultoId = adultoId,
                MesReferencia = mesAnterior,
                ValorTotal = fiado,
                Status = fiado > 0 ? StatusFechamento.Aberto : StatusFechamento.Pago,
                GeradoEm = agora,
                PagoEm = fiado > 0 ? null : agora,
            });
        }

        await _db.SaveChangesAsync();

        return adultosSemFechamento.Count;
    }

    /// <summary>
    /// Paga o fechamento (pagamento simulado, D1): quita o fiado de cada conta da família, da mais
    /// negativa para a menos, até completar o valor. Se sobrar (a família já pagou parte), vira crédito do responsável.
    /// </summary>
    public async Task Pagar(int fechamentoId, int adultoId)
    {
        var fechamento = await _db.Fechamentos.FirstOrDefaultAsync(f => f.Id == fechamentoId && f.AdultoId == adultoId)
            ?? throw new RegraException("Fechamento não encontrado");

        if (fechamento.Status == StatusFechamento.Pago)
            throw new RegraException("Fechamento já está pago");

        var descricao = $"Pagamento do fechamento {fechamento.MesReferencia:MM/yyyy}";
        var restante = fechamento.ValorTotal;

        var contasNegativas = await ContasDaFamilia(adultoId)
            .Where(c => c.Saldo < 0)
            .OrderBy(c => c.Saldo)
            .ToListAsync();

        foreach (var conta in contasNegativas)
        {
            var valor = Math.Min(restante, -conta.Saldo);
            if (valor <= 0)
                break;

            await _conta.Movimentar(conta.UsuarioId, valor, TipoMovimento.Pagamento, descricao);
            restante -= valor;
        }

        if (restante > 0)
            await _conta.Movimentar(adultoId, restante, TipoMovimento.Pagamento, descricao);

        fechamento.Status = StatusFechamento.Pago;
        fechamento.PagoEm = DateTime.Now;

        await _db.SaveChangesAsync();
    }

    // Conta do responsável e dos filhos dele
    private IQueryable<Conta> ContasDaFamilia(int adultoId) =>
        _db.Contas.Where(c =>
            c.UsuarioId == adultoId ||
            _db.Alunos.Any(a => a.UsuarioId == c.UsuarioId && a.AdultoId == adultoId));
}
