using System.ComponentModel.DataAnnotations;

namespace Backend.Domain.DTOs.Pedidos;

// Pedido antecipado feito pelo próprio aluno ou adulto
public class CriarPedidoDto
{
    // Dia da retirada (hoje ou um dia futuro)
    public DateOnly Data { get; set; }

    public int IntervaloId { get; set; }

    [MinLength(1, ErrorMessage = "Escolha pelo menos um item.")]
    public List<ItemQuantidadeDto> Itens { get; set; } = [];
}
