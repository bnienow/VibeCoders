namespace Backend.Models;

public class Aluno
{
    public int UsuarioId { get; set; }
    public int AdultoId { get; set; }
    public string Turma { get; set; } = string.Empty;
    public DateOnly DataNascimento { get; set; }

    // Null = sem limite diário
    public decimal? LimiteDiario { get; set; }

    // Alérgenos separados por vírgula, ex.: "Gluten,Lactose"
    public string RestricoesAlimentares { get; set; } = string.Empty;

    public Usuario Usuario { get; set; } = null!;
    public Adulto Adulto { get; set; } = null!;
}
