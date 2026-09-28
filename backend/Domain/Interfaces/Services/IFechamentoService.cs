using Backend.Domain.DTOs.Fechamentos;

namespace Backend.Domain.Interfaces.Services;

/// <summary>
/// Fechamento mensal (R7). O valor a pagar é o fiado: a soma dos saldos negativos das contas da família
/// (responsável + filhos). O consumo do mês aparece só como informativo.
/// </summary>
public interface IFechamentoService
{
    // Do mais recente para o mais antigo, com o consumo item a item. Nulo se não for o responsável nem o Admin
    Task<List<FechamentoDto>?> DoAdulto(int adultoId, int usuarioId, bool ehAdmin);

    // Gera o fechamento do mês anterior para quem ainda não tem (dia 1º). Devolve quantos foram criados
    Task<int> GerarMesAnterior();

    // Pagamento simulado (D1)
    Task Pagar(int fechamentoId, int adultoId);
}
