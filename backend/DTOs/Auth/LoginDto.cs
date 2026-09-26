using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Auth;

public class LoginDto
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Senha { get; set; } = string.Empty;
}
