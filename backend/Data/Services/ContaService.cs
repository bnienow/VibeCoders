using Backend.Domain.Exceptions;
using Backend.Domain.Interfaces.Repositories;
using Backend.Domain.Interfaces.Services;
using Backend.Domain.Models;
using Backend.Domain.Models.Enums;

namespace Backend.Data.Services;

public class ContaService : IContaService
{
    private readonly IContaRepository _contas;
    private readonly IAlunoRepository _alunos;
    private readonly IMetodoPagamentoRepository _metodos;

    public ContaService(IContaRepository contas, IAlunoRepository alunos, IMetodoPagamentoRepository metodos)
    {
        _contas = contas;
        _alunos = alunos;
        _metodos = metodos;
    }

    /// <summary>
    /// Soma o valor no saldo e registra o Movimento com o saldo resultante.
    /// </summary>
    public async Task Movimentar(int usuarioId, decimal valor, TipoMovimento tipo, string descricao, Pedido? pedido = null)
    {
        var conta = await _contas.BuscarPorUsuario(usuarioId)
            ?? throw new RegraException("Conta não encontrada");

        conta.Saldo += valor;
        conta.AtualizadaEm = DateTime.Now;

        _contas.AdicionarMovimento(new Movimento
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

    public async Task AdicionarCredito(int adultoId, int destinoId, decimal valor, int? metodoPagamentoId)
    {
        var destinoValido = destinoId == adultoId || await _alunos.EhFilho(destinoId, adultoId);
        if (!destinoValido)
            throw new RegraException("Só é possível colocar crédito na sua conta ou na de um filho");

        var metodo = metodoPagamentoId == null
            ? null
            : await _metodos.BuscarAtivo(metodoPagamentoId.Value, adultoId)
              ?? throw new RegraException("Método de pagamento não encontrado");

        await Movimentar(destinoId, valor, TipoMovimento.Credito, $"Crédito via {metodo?.Apelido ?? "pagamento simulado"}");
    }
}
