using Backend.Domain.Models.Enums;

namespace Backend.Domain.DTOs.Auth;

public class UsuarioLogadoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Permissao Permissao { get; set; }
}
