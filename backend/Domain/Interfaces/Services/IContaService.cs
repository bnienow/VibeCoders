using Backend.Domain.Models;
using Backend.Domain.Models.Enums;

namespace Backend.Domain.Interfaces.Services;

/// <summary>
/// Conta corrente: todo dinheiro que entra ou sai passa por aqui e vira um Movimento (extrato).
/// Não grava: quem chama salva junto com o resto da operação, numa transação só.
/// </summary>
public interface IContaService
{
    // Valor negativo = saiu (compra); positivo = entrou (crédito, pagamento, estorno)
    Task Movimentar(int usuarioId, decimal valor, TipoMovimento tipo, string descricao, Pedido? pedido = null);

    // Crédito na conta do responsável ou de um filho, com pagamento simulado (D1: sempre aprovado)
    Task AdicionarCredito(int adultoId, int destinoId, decimal valor, int? metodoPagamentoId);
}
