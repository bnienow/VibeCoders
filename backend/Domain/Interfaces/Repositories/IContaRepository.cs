using Backend.Domain.Models;

namespace Backend.Domain.Interfaces.Repositories;

/// <summary>
/// Conta do usuário e seus movimentos (extrato).
/// </summary>
public interface IContaRepository
{
    Task<Conta?> BuscarPorUsuario(int usuarioId);
    Task<bool> Existe(int usuarioId);

    // Soma dos saldos negativos da conta do responsável e dos filhos
    Task<decimal> FiadoDaFamilia(int adultoId);

    // Contas negativas da família, da mais negativa para a menos
    Task<List<Conta>> NegativasDaFamilia(int adultoId);

    // Gasto líquido do mês por usuário (compras menos estornos)
    Task<Dictionary<int, decimal>> GastosDesde(List<int> usuarioIds, DateTime inicio);

    // Movimentos no período, com o pedido e seus itens
    Task<List<Movimento>> MovimentosDoPeriodo(int usuarioId, DateTime inicio, DateTime fim);

    void Adicionar(Conta conta);
    void AdicionarMovimento(Movimento movimento);
}
