using Backend.Domain.DTOs.Auth;

namespace Backend.Domain.Interfaces.Services;

public interface IAuthService
{
    // Nulo se o e-mail não existe, está inativo ou a senha não confere
    Task<UsuarioLogadoDto?> Login(LoginDto dto);

    // Responsável: Usuario + Adulto + Conta com saldo zero. E-mail em uso → ConflitoException
    Task<UsuarioLogadoDto> Registrar(RegistroDto dto);

    // Nulo se o usuário foi desativado ou apagado depois do login
    Task<UsuarioLogadoDto?> BuscarAtivo(int id);
}
