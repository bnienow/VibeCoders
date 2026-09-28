using Backend.Domain.Models.Enums;

namespace Backend.Domain.Models;

public class MetodoPagamento
{
    public int Id { get; set; }
    public int AdultoId { get; set; }
    public TipoMetodoPagamento Tipo { get; set; }
    public string Apelido { get; set; } = string.Empty;

    // Dados fictícios
    public string UltimosDigitos { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;

    public Adulto Adulto { get; set; } = null!;
}
