using Backend.Domain.DTOs.Extratos;
using Backend.Domain.DTOs.Pedidos;
using Backend.Domain.Exceptions;
using Backend.Domain.Interfaces.Repositories;
using Backend.Domain.Interfaces.Services;
using Backend.Domain.Models.Enums;

namespace Backend.Data.Services;

public class ExtratoService : IExtratoService
{
    private readonly IAlunoRepository _alunos;
    private readonly IContaRepository _contas;

    public ExtratoService(IAlunoRepository alunos, IContaRepository contas)
    {
        _alunos = alunos;
        _contas = contas;
    }

    public async Task<ExtratoDto?> DoAluno(int alunoId, string? mes, int usuarioId, bool ehAdmin)
    {
        var aluno = await _alunos.BuscarComConta(alunoId);
        if (aluno == null || !aluno.VisivelPara(usuarioId, ehAdmin))
            return null;

        var inicio = PrimeiroDiaDoMes(mes);
        var inicioDoMes = inicio.ToDateTime(TimeOnly.MinValue);

        var movimentos = await _contas.MovimentosDoPeriodo(aluno.UsuarioId, inicioDoMes, inicioDoMes.AddMonths(1));

        return new ExtratoDto
        {
            AlunoId = aluno.UsuarioId,
            AlunoNome = aluno.Usuario.Nome,
            Mes = inicio,
            SaldoAtual = aluno.Usuario.Conta?.Saldo ?? 0,
            TotalGasto = -movimentos.Where(m => m.Tipo is TipoMovimento.Compra or TipoMovimento.Estorno).Sum(m => m.Valor),
            TotalCreditos = movimentos.Where(m => m.Tipo is TipoMovimento.Credito or TipoMovimento.Pagamento).Sum(m => m.Valor),
            Movimentos = movimentos.Select(m => new MovimentoDto
            {
                Data = m.Data,
                Tipo = m.Tipo,
                Valor = m.Valor,
                Descricao = m.Descricao,
                SaldoApos = m.SaldoApos,
                Itens = m.Pedido?.Itens.Select(ip => new ItemPedidoDto
                {
                    ItemId = ip.ItemId,
                    Nome = ip.Item.Nome,
                    Quantidade = ip.Quantidade,
                    PrecoUnitario = ip.PrecoUnitario,
                    Subtotal = ip.Subtotal,
                }).ToList() ?? [],
            }).ToList(),
        };
    }

    // "2026-08" → 01/08/2026; vazio → primeiro dia do mês atual
    private static DateOnly PrimeiroDiaDoMes(string? mes)
    {
        if (string.IsNullOrEmpty(mes))
        {
            var hoje = DateOnly.FromDateTime(DateTime.Now);
            return new DateOnly(hoje.Year, hoje.Month, 1);
        }

        if (!DateOnly.TryParseExact(mes + "-01", "yyyy-MM-dd", out var inicio))
            throw new RegraException("Mês inválido. Use o formato 2026-08");

        return inicio;
    }
}
