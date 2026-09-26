namespace Backend.DTOs.Pedidos;

// Resultado de cada venda offline reenviada (D6)
public class ResultadoSincronizacaoDto
{
    // Posição da venda na lista enviada
    public int Indice { get; set; }

    // "Aceita", "ConvertidaParaAVista" (estourou o limite offline: conferir) ou "Rejeitada"
    public string Situacao { get; set; } = string.Empty;

    public int? PedidoId { get; set; }

    // Por que não entrou como foi enviada
    public string? Motivo { get; set; }
}
