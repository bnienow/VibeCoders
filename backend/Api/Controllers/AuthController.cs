using System.Security.Claims;
using Backend.Api.Extensions;
using Backend.Domain.DTOs.Auth;
using Backend.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

/// <summary>
/// Autenticação por cookie: login, cadastro de responsável, logout e usuário logado.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    // Mesma mensagem para e-mail inexistente e senha errada: não revela quais e-mails existem
    private const string CredenciaisInvalidas = "E-mail ou senha inválidos";

    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    /// <summary>
    /// Confere e-mail e senha e cria o cookie de sessão.
    /// </summary>
    /// <returns>200 com o usuário logado, ou 401 se as credenciais forem inválidas.</returns>
    [HttpPost("login")]
    public async Task<ActionResult<UsuarioLogadoDto>> Login(LoginDto dto)
    {
        var usuario = await _auth.Login(dto);
        if (usuario is null)
            return Unauthorized(CredenciaisInvalidas);

        await CriarSessao(usuario);

        return usuario;
    }

    /// <summary>
    /// Cadastra um responsável (Usuario + Adulto + Conta com saldo zero).
    /// Aluno é cadastrado pelo responsável logado; Admin não se cadastra.
    /// </summary>
    /// <returns>200 com o usuário criado, 409 se o e-mail já existe, ou 400 se os dados forem inválidos.</returns>
    [HttpPost("registro")]
    public Task<UsuarioLogadoDto> Registro(RegistroDto dto) => _auth.Registrar(dto);

    /// <summary>
    /// Encerra a sessão apagando o cookie.
    /// </summary>
    /// <returns>204.</returns>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();

        return NoContent();
    }

    /// <summary>
    /// Devolve o usuário da sessão atual. O front chama ao abrir para saber se ainda há alguém logado.
    /// </summary>
    /// <returns>200 com o usuário, ou 401 se não houver sessão ou o usuário tiver sido desativado.</returns>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UsuarioLogadoDto>> Me()
    {
        var usuario = await _auth.BuscarAtivo(User.UsuarioId());
        if (usuario is null)
            return Unauthorized();

        return usuario;
    }

    /// <summary>
    /// Grava no cookie quem é o usuário (Id, nome e permissão).
    /// A permissão vira Role, o que habilita [Authorize(Roles = "Admin")] nas outras rotas.
    /// </summary>
    private async Task CriarSessao(UsuarioLogadoDto usuario)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Role, usuario.Permissao.ToString()),
        };

        var identidade = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(new ClaimsPrincipal(identidade));
    }
}
