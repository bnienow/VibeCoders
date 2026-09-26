using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Auth;

// Cadastro de responsável. Aluno é cadastrado pelo responsável; Admin não se cadastra.
public class RegistroDto
{
    [Required(ErrorMessage = "Informe o nome completo.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o e-mail."), EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a senha."), MinLength(6, ErrorMessage = "A senha precisa ter pelo menos 6 caracteres.")]
    public string Senha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o CPF.")]
    [RegularExpression(@"^\d{3}\.\d{3}\.\d{3}-\d{2}$", ErrorMessage = "CPF deve estar no formato 000.000.000-00.")]
    public string Cpf { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o telefone.")]
    [RegularExpression(@"^\(\d{2}\) \d{4,5}-\d{4}$", ErrorMessage = "Telefone deve estar no formato (00) 00000-0000.")]
    public string Telefone { get; set; } = string.Empty;
}
