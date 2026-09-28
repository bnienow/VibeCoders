using Backend.Domain.DTOs.Pedidos;

namespace Backend.Domain.Interfaces.Services;

/// <summary>
/// Regras de pedido (spec, seções 4 e 10): janela, estoque, limite diário, teto de fiado e lançamento na conta.
/// Regra violada vira RegraException (400).
/// </summary>
public interface IPedidoService
{
    // Pedido antecipado: valida janela, disponibilidade, duplicidade e limites; debita a conta na hora
    Task<PedidoDto> CriarAntecipado(int usuarioId, CriarPedidoDto dto);

    // Venda no balcão: sem janela e sem cardápio do intervalo. Nasce Entregue
    Task<PedidoDto> VenderNoBalcao(VendaBalcaoDto dto);

    // Vendas de balcão feitas sem internet (D6), reprocessadas com as regras normais
    Task<List<ResultadoSincronizacaoDto>> Sincronizar(List<VendaBalcaoDto> vendas);

    // Do mais recente para o mais antigo
    Task<List<PedidoDto>> DoUsuario(int usuarioId);

    // Nulo se não existe ou se não é do usuário (Admin vê todos)
    Task<PedidoDto?> Detalhe(int id, int usuarioId, bool ehAdmin);

    // Troca os itens enquanto a janela estiver aberta
    Task<PedidoDto> Alterar(int pedidoId, int usuarioId, AlterarPedidoDto dto);

    // Cancela e estorna enquanto a janela estiver aberta
    Task Cancelar(int pedidoId, int usuarioId);

    // Painel da cantina
    Task Entregar(int pedidoId);
}
