using Backend.Domain.DTOs.Painel;
using Backend.Domain.DTOs.Pedidos;

namespace Backend.Domain.Interfaces.Services;

/// <summary>
/// Painel da cantina (cena 2): pedidos antecipados do intervalo em andamento ou do próximo a começar.
/// </summary>
public interface IPainelService
{
    Task<PainelDto> IntervaloAtual();

    // Consolidado por item dos pedidos ainda não entregues (ex.: "12x Pão de queijo")
    Task<List<PreparoItemDto>> Preparo();

    // Pedidos antecipados de hoje por código de retirada, nome ou e-mail (R4)
    Task<List<PedidoDto>> Buscar(string termo);
}
