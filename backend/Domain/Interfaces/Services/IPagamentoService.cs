using Backend.Domain.DTOs.Pagamentos;

namespace Backend.Domain.Interfaces.Services;

/// <summary>
/// Métodos de pagamento e crédito simulado (D1). Nada aqui fala com gateway real: todo pagamento é aprovado.
/// </summary>
public interface IPagamentoService
{
    // Métodos ativos do responsável, indicando o padrão
    Task<List<MetodoPagamentoDto>> Metodos(int adultoId);

    // Pix ou cartão fictício. O primeiro cadastrado vira padrão automaticamente
    Task<MetodoPagamentoDto> Cadastrar(int adultoId, CreateMetodoPagamentoDto dto);

    // Credita na conta do responsável ou de um filho
    Task Simular(int adultoId, SimularPagamentoDto dto);
}
