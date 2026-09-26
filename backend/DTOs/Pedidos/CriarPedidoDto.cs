using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Pedidos;

// Pedido antecipado feito pelo próprio aluno ou adulto
public class CriarPedidoDto
{
    // Dia da retirada (hoje ou um dia futuro)
    public DateOnly Data { get; set; }

    public int IntervaloId { get; set; }

    [MinLength(1)]
    public List<ItemQuantidadeDto> Itens { get; set; } = [];
}
