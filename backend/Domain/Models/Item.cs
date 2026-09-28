using Backend.Domain.Models.Enums;

namespace Backend.Domain.Models;

public class Item
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal PrecoUnitario { get; set; }

    // 0 = esgotado, some do cardápio
    public int Estoque { get; set; }

    public CategoriaItem Categoria { get; set; }

    // Alérgenos separados por vírgula, ex.: "Gluten,Lactose,Amendoim"
    public string Alergenos { get; set; } = string.Empty;

    // Desativa sem apagar histórico
    public bool Ativo { get; set; } = true;
}
