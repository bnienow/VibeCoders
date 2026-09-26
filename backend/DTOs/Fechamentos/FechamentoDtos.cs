using Backend.Models.Enums;

namespace Backend.DTOs.Fechamentos;

// Fatura do mês do responsável
public class FechamentoDto
{
    public int Id { get; set; }
    public DateOnly MesReferencia { get; set; }

    // Valor a pagar: o fiado (saldo negativo) das contas da família quando o fechamento foi gerado
    public decimal ValorTotal { get; set; }

    public StatusFechamento Status { get; set; }
    public DateTime? GeradoEm { get; set; }
    public DateTime? PagoEm { get; set; }

    // Consumo do mês na conta, por filho e item a item (informativo)
    public List<ConsumoDto> Consumo { get; set; } = [];
}

public class ConsumoDto
{
    public string Nome { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public List<ConsumoItemDto> Itens { get; set; } = [];
}

public class ConsumoItemDto
{
    public string Nome { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal Total { get; set; }
}
