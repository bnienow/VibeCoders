using Backend.Domain.Models.Enums;

namespace Backend.Domain.DTOs.Itens;

// O que a API devolve de um item (cardápio e gestão usam o mesmo formato)
public class ItemDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal PrecoUnitario { get; set; }
    public int Estoque { get; set; }
    public CategoriaItem Categoria { get; set; }
    public string[] Alergenos { get; set; } = [];
    public bool Ativo { get; set; }

    // True quando o item tem alérgeno que o aluno logado não pode comer (D3)
    public bool ConflitaComRestricao { get; set; }
}
