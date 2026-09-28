using Backend.Domain.Models.Enums;

namespace Backend.Domain.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public Permissao Permissao { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; }

    public Aluno? Aluno { get; set; }
    public Adulto? Adulto { get; set; }
    public Conta? Conta { get; set; }
}
