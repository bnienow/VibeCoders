using System.ComponentModel.DataAnnotations;

namespace Backend.Domain.DTOs.Pedidos;

// Um item do carrinho: qual e quantos
public class ItemQuantidadeDto
{
    public int ItemId { get; set; }

    [Range(1, 99, ErrorMessage = "Quantidade precisa ser entre 1 e 99.")]
    public int Quantidade { get; set; }
}
