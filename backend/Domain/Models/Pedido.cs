using Backend.Domain.Models.Enums;

namespace Backend.Domain.Models;

public class Pedido
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public DateOnly Data { get; set; }

    // Null = venda de balcão fora de intervalo
    public int? IntervaloId { get; set; }

    public StatusPedido Status { get; set; }
    public TipoVenda TipoVenda { get; set; }
    public FormaPagamento FormaPagamento { get; set; }
    public decimal Total { get; set; }
    public string CodigoRetirada { get; set; } = string.Empty;
    public bool TemAlertaAlergia { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? EntregueEm { get; set; }
    public DateTime? CanceladoEm { get; set; }

    public Usuario Usuario { get; set; } = null!;
    public Intervalo? Intervalo { get; set; }
    public ICollection<ItemPedido> Itens { get; set; } = new List<ItemPedido>();

    /// <summary>
    /// Status para mostrar: um pedido Aberto cuja janela já fechou é Confirmado
    /// (a cozinha já está montando). Calculado na leitura, sem job rodando.
    /// </summary>
    public StatusPedido StatusAtual() =>
        Status == StatusPedido.Aberto && !JanelaAberta() ? StatusPedido.Confirmado : Status;

    /// <summary>
    /// Pode alterar ou cancelar: pedido Aberto e janela ainda aberta.
    /// </summary>
    public bool PodeAlterar() => Status == StatusPedido.Aberto && JanelaAberta();

    // Precisa do Intervalo carregado
    private bool JanelaAberta() => Intervalo != null && DateTime.Now < Intervalo.FechamentoEm(Data);
}
