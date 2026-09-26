using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Auth;

// Cadastro de responsável. Aluno é cadastrado pelo responsável; Admin não se cadastra.
public class RegistroDto
{
    [Required]
    public string Nome { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Senha { get; set; } = string.Empty;

    [Required]
    public string Cpf { get; set; } = string.Empty;

    [Required]
    public string Telefone { get; set; } = string.Empty;
}
