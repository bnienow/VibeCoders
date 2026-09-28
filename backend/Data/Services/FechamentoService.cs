using Backend.Domain.DTOs.Fechamentos;
using Backend.Domain.Exceptions;
using Backend.Domain.Interfaces.Repositories;
using Backend.Domain.Interfaces.Services;
using Backend.Domain.Models;
using Backend.Domain.Models.Enums;

namespace Backend.Data.Services;

public class FechamentoService : IFechamentoService
{
    private readonly IFechamentoRepository _fechamentos;
    private readonly IAdultoRepository _adultos;
    private readonly IContaRepository _contas;
    private readonly IPedidoRepository _pedidos;
    private readonly IContaService _conta;
    private readonly IUnitOfWork _uow;

    public FechamentoService(IFechamentoRepository fechamentos, IAdultoRepository adultos, IContaRepository contas,
        IPedidoRepository pedidos, IContaService conta, IUnitOfWork uow)
    {
        _fechamentos = fechamentos;
        _adultos = adultos;
        _contas = contas;
        _pedidos = pedidos;
        _conta = conta;
        _uow = uow;
    }

    public async Task<List<FechamentoDto>?> DoAdulto(int adultoId, int usuarioId, bool ehAdmin)
    {
        if (adultoId != usuarioId && !ehAdmin)
            return null;

        var resultado = new List<FechamentoDto>();
        foreach (var f in await _fechamentos.DoAdulto(adultoId))
        {
            resultado.Add(new FechamentoDto
            {
                Id = f.Id,
                MesReferencia = f.MesReferencia,
                ValorTotal = f.ValorTotal,
                Status = f.Status,
                GeradoEm = f.GeradoEm,
                PagoEm = f.PagoEm,
                Consumo = await ConsumoDoMes(adultoId, f.MesReferencia),
            });
        }

        return resultado;
    }

    /// <summary>
    /// Quem não tem fiado recebe o fechamento já como Pago.
    /// </summary>
    public async Task<int> GerarMesAnterior()
    {
        var hoje = DateOnly.FromDateTime(DateTime.Now);
        var mesAnterior = new DateOnly(hoje.Year, hoje.Month, 1).AddMonths(-1);
        var agora = DateTime.Now;

        var adultosSemFechamento = await _adultos.IdsSemFechamento(mesAnterior);

        foreach (var adultoId in adultosSemFechamento)
        {
            var fiado = await _contas.FiadoDaFamilia(adultoId);

            _fechamentos.Adicionar(new Fechamento
            {
                AdultoId = adultoId,
                MesReferencia = mesAnterior,
                ValorTotal = fiado,
                Status = fiado > 0 ? StatusFechamento.Aberto : StatusFechamento.Pago,
                GeradoEm = agora,
                PagoEm = fiado > 0 ? null : agora,
            });
        }

        await _uow.SalvarAsync();

        return adultosSemFechamento.Count;
    }

    /// <summary>
    /// Quita o fiado de cada conta da família, da mais negativa para a menos, até completar o valor.
    /// Se sobrar (a família já pagou parte), vira crédito do responsável.
    /// </summary>
    public async Task Pagar(int fechamentoId, int adultoId)
    {
        var fechamento = await _fechamentos.BuscarDoAdulto(fechamentoId, adultoId)
            ?? throw new RegraException("Fechamento não encontrado");

        if (fechamento.Status == StatusFechamento.Pago)
            throw new RegraException("Fechamento já está pago");

        var descricao = $"Pagamento do fechamento {fechamento.MesReferencia:MM/yyyy}";
        var restante = fechamento.ValorTotal;

        foreach (var conta in await _contas.NegativasDaFamilia(adultoId))
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

        await _uow.SalvarAsync();
    }

    // O que a família consumiu na conta no mês, por pessoa e item a item
    private async Task<List<ConsumoDto>> ConsumoDoMes(int adultoId, DateOnly mes)
    {
        var itens = await _pedidos.ConsumoDaFamilia(adultoId, mes, mes.AddMonths(1));

        return itens
            .GroupBy(i => i.Pessoa)
            .Select(pessoa => new ConsumoDto
            {
                Nome = pessoa.Key,
                Total = pessoa.Sum(i => i.Subtotal),
                Itens = pessoa
                    .GroupBy(i => i.Item)
                    .Select(item => new ConsumoItemDto
                    {
                        Nome = item.Key,
                        Quantidade = item.Sum(i => i.Quantidade),
                        Total = item.Sum(i => i.Subtotal),
                    })
                    .OrderByDescending(i => i.Total)
                    .ToList(),
            })
            .ToList();
    }
}
