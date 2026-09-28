namespace Backend.Domain.Models;

public class Aluno
{
    // Todo aluno tem e-mail do domínio da escola (fictício)
    public const string DominioEmail = "aluno.cantina.test";

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

    // Veem o aluno: ele mesmo, o responsável dele e o Admin
    public bool VisivelPara(int usuarioId, bool ehAdmin) =>
        ehAdmin || UsuarioId == usuarioId || AdultoId == usuarioId;
}
