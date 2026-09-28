using System.ComponentModel.DataAnnotations;

namespace Backend.Domain.DTOs.Pedidos;

// Novo conteúdo do pedido: substitui a lista de itens inteira
public class AlterarPedidoDto
{
    [MinLength(1, ErrorMessage = "Escolha pelo menos um item.")]
    public List<ItemQuantidadeDto> Itens { get; set; } = [];
}
