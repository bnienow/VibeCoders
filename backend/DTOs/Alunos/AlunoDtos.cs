using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Alunos;

// O que a API devolve de um aluno (balcão, painel do responsável, detalhe)
public class AlunoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Turma { get; set; } = string.Empty;
    public DateOnly DataNascimento { get; set; }
    public int AdultoId { get; set; }
    public decimal Saldo { get; set; }
    public decimal? LimiteDiario { get; set; }
    public string[] Restricoes { get; set; } = [];

    // Compras do mês corrente, já descontados os estornos (card do responsável)
    public decimal GastoMes { get; set; }
}

// Cadastro de filho, feito pelo responsável logado
public class CreateAlunoDto
{
    [Required]
    public string Nome { get; set; } = string.Empty;

    // Precisa ser do domínio da escola (Aluno.DominioEmail)
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Senha { get; set; } = string.Empty;

    [Required]
    public string Turma { get; set; } = string.Empty;

    public DateOnly DataNascimento { get; set; }

    public string[] Restricoes { get; set; } = [];
}

public class LimiteDto
{
    // Nulo = sem limite
    [Range(0.01, 9999)]
    public decimal? LimiteDiario { get; set; }
}

public class RestricoesDto
{
    public string[] Restricoes { get; set; } = [];
}

public class CreditoDto
{
    [Range(1, 1000)]
    public decimal Valor { get; set; }

    public int? MetodoPagamentoId { get; set; }
}
