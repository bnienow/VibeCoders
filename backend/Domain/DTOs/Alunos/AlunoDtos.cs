using System.ComponentModel.DataAnnotations;

namespace Backend.Domain.DTOs.Alunos;

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
    [Required(ErrorMessage = "Informe o nome completo.")]
    public string Nome { get; set; } = string.Empty;

    // Precisa ser do domínio da escola (Aluno.DominioEmail)
    [Required(ErrorMessage = "Informe o e-mail."), EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a senha."), MinLength(6, ErrorMessage = "A senha precisa ter pelo menos 6 caracteres.")]
    public string Senha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a turma.")]
    public string Turma { get; set; } = string.Empty;

    public DateOnly DataNascimento { get; set; }

    public string[] Restricoes { get; set; } = [];
}

public class LimiteDto
{
    // Nulo = sem limite
    [Range(0.01, 9999, ErrorMessage = "O limite precisa ser maior que zero.")]
    public decimal? LimiteDiario { get; set; }
}

public class RestricoesDto
{
    public string[] Restricoes { get; set; } = [];
}

public class CreditoDto
{
    [Range(1, 1000, ErrorMessage = "O crédito precisa ser entre R$ 1,00 e R$ 1.000,00.")]
    public decimal Valor { get; set; }

    public int? MetodoPagamentoId { get; set; }
}
