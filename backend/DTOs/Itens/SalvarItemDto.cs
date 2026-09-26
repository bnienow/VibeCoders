using System.ComponentModel.DataAnnotations;
using Backend.Models.Enums;

namespace Backend.DTOs.Itens;

// O que o Admin envia para criar ou editar um item
public class SalvarItemDto
{
    [Required]
    public string Nome { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    [Range(0.01, 9999)]
    public decimal PrecoUnitario { get; set; }

    [Range(0, int.MaxValue)]
    public int Estoque { get; set; }

    public CategoriaItem Categoria { get; set; }

    public string[] Alergenos { get; set; } = [];
}
