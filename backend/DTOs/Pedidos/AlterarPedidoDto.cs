using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Pedidos;

// Novo conteúdo do pedido: substitui a lista de itens inteira
public class AlterarPedidoDto
{
    [MinLength(1)]
    public List<ItemQuantidadeDto> Itens { get; set; } = [];
}
