using Backend.Domain.DTOs.Relatorios;
using Backend.Domain.Models;

namespace Backend.Domain.Interfaces.Repositories;

// Uma linha do que a família consumiu: quem, qual item, quanto
public record ConsumoLinha(string Pessoa, string Item, int Quantidade, decimal Subtotal);

public interface IPedidoRepository
{
    // Leitura com comprador, intervalo e itens (o que o PedidoDto precisa), sem rastrear
    Task<Pedido?> BuscarComDetalhes(int id);
    Task<List<Pedido>> DoUsuario(int usuarioId);

    // Antecipados não cancelados de hoje no intervalo, na ordem em que foram feitos
    Task<List<Pedido>> AntecipadosDoIntervalo(DateOnly dia, int intervaloId);

    // Antecipados não cancelados do dia por código de retirada, nome ou e-mail
    Task<List<Pedido>> BuscarAntecipados(DateOnly dia, string termo);

    // Para alterar/cancelar: rastreado, com intervalo e itens
    Task<Pedido?> BuscarParaEditar(int id, int usuarioId);

    Task<Pedido?> BuscarPorId(int id);

    Task<bool> TemAntecipado(int usuarioId, DateOnly data, int intervaloId);

    // Soma dos pedidos na conta do dia, sem contar o pedido ignorado
    Task<decimal> GastoNaContaDoDia(int usuarioId, DateOnly data, int? ignorarPedidoId);

    Task<bool> CodigoEmUso(DateOnly data, string codigo);

    // Itens pedidos na conta pela família no período (sem cancelados)
    Task<List<ConsumoLinha>> ConsumoDaFamilia(int adultoId, DateOnly inicio, DateOnly fim);

    Task<List<VendaDiaDto>> VendasPorDia(DateOnly de, DateOnly ate);
    Task<List<ItemVendidoDto>> RankingDeItens(DateOnly de, DateOnly ate);

    void Adicionar(Pedido pedido);
}
