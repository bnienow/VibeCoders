using Backend.Domain.DTOs.Pagamentos;
using Backend.Domain.Interfaces.Repositories;
using Backend.Domain.Interfaces.Services;
using Backend.Domain.Models;

namespace Backend.Data.Services;

public class PagamentoService : IPagamentoService
{
    private readonly IAdultoRepository _adultos;
    private readonly IMetodoPagamentoRepository _metodos;
    private readonly IContaService _conta;
    private readonly IUnitOfWork _uow;

    public PagamentoService(IAdultoRepository adultos, IMetodoPagamentoRepository metodos, IContaService conta, IUnitOfWork uow)
    {
        _adultos = adultos;
        _metodos = metodos;
        _conta = conta;
        _uow = uow;
    }

    public async Task<List<MetodoPagamentoDto>> Metodos(int adultoId)
    {
        var adulto = await _adultos.BuscarComMetodos(adultoId);

        return adulto.MetodosPagamento
            .Where(m => m.Ativo)
            .Select(m => ParaDto(m, adulto))
            .ToList();
    }

    public async Task<MetodoPagamentoDto> Cadastrar(int adultoId, CreateMetodoPagamentoDto dto)
    {
        var adulto = await _adultos.Buscar(adultoId);

        var metodo = new MetodoPagamento
        {
            Adulto = adulto,
            Tipo = dto.Tipo,
            Apelido = dto.Apelido,
            UltimosDigitos = dto.UltimosDigitos,
        };
        _metodos.Adicionar(metodo);

        if (dto.Padrao || adulto.MetodoPagamentoPadraoId == null)
            adulto.MetodoPagamentoPadrao = metodo;

        await _uow.SalvarAsync();

        return ParaDto(metodo, adulto);
    }

    public async Task Simular(int adultoId, SimularPagamentoDto dto)
    {
        await _conta.AdicionarCredito(adultoId, dto.UsuarioId, dto.Valor, dto.MetodoPagamentoId);
        await _uow.SalvarAsync();
    }

    private static MetodoPagamentoDto ParaDto(MetodoPagamento m, Adulto adulto) => new()
    {
        Id = m.Id,
        Tipo = m.Tipo,
        Apelido = m.Apelido,
        UltimosDigitos = m.UltimosDigitos,
        Padrao = adulto.MetodoPagamentoPadraoId == m.Id,
    };
}
