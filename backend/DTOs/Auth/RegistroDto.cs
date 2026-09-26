using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Auth;

// Cadastro de responsável. Aluno é cadastrado pelo responsável logado; Admin não se cadastra.
public class RegistroDto
{
    [Required, MaxLength(255)]
    public string Nome { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Senha { get; set; } = string.Empty;

    [Required, MaxLength(14)]
    public string Cpf { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Telefone { get; set; } = string.Empty;
}
