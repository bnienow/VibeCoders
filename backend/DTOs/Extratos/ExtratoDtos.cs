using Backend.DTOs.Pedidos;
using Backend.Models.Enums;

namespace Backend.DTOs.Extratos;

// Extrato do mês de um aluno (cena 4)
public class ExtratoDto
{
    public int AlunoId { get; set; }
    public string AlunoNome { get; set; } = string.Empty;

    // Primeiro dia do mês do extrato
    public DateOnly Mes { get; set; }

    public decimal SaldoAtual { get; set; }

    // Compras menos estornos no mês
    public decimal TotalGasto { get; set; }

    // Créditos e pagamentos no mês
    public decimal TotalCreditos { get; set; }

    public List<MovimentoDto> Movimentos { get; set; } = [];
}

public class MovimentoDto
{
    public DateTime Data { get; set; }
    public TipoMovimento Tipo { get; set; }
    public decimal Valor { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal SaldoApos { get; set; }

    // Itens comprados, quando o movimento é de um pedido (item a item)
    public List<ItemPedidoDto> Itens { get; set; } = [];
}
