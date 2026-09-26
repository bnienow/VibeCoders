using System.Security.Claims;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.Extensions;
using Backend.Models;
using Backend.Models.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

/// <summary>
/// Autenticação por cookie: login, cadastro de responsável, logout e usuário logado.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    // Mesma mensagem para e-mail inexistente e senha errada: não revela quais e-mails existem
    private const string CredenciaisInvalidas = "E-mail ou senha inválidos";

    private readonly AppDbContext _db;
    private readonly IPasswordHasher<Usuario> _hasher;

    public AuthController(AppDbContext db, IPasswordHasher<Usuario> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    /// <summary>
    /// Confere e-mail e senha e cria o cookie de sessão.
    /// </summary>
    /// <returns>200 com o usuário logado, ou 401 se as credenciais forem inválidas.</returns>
    [HttpPost("login")]
    public async Task<ActionResult<UsuarioLogadoDto>> Login(LoginDto dto)
    {
        var usuario = await _db.Usuarios
            .SingleOrDefaultAsync(u => u.Email == dto.Email && u.Ativo);

        if (usuario is null || !SenhaConfere(usuario, dto.Senha))
            return Unauthorized(CredenciaisInvalidas);

        await CriarSessao(usuario);

        return ParaDto(usuario);
    }

    /// <summary>
    /// Cadastra um responsável (Usuario + Adulto + Conta com saldo zero).
    /// Aluno é cadastrado pelo responsável logado; Admin não se cadastra.
    /// </summary>
    /// <returns>200 com o usuário criado, 409 se o e-mail já existe, ou 400 se os dados forem inválidos.</returns>
    [HttpPost("registro")]
    public async Task<ActionResult<UsuarioLogadoDto>> Registro(RegistroDto dto)
    {
        var emailEmUso = await _db.Usuarios.AnyAsync(u => u.Email == dto.Email);
        if (emailEmUso)
            return Conflict("E-mail já cadastrado");

        var agora = DateTime.Now;

        var usuario = new Usuario
        {
            Nome = dto.Nome,
            Email = dto.Email,
            Permissao = Permissao.Adulto,
            CriadoEm = agora,
        };
        usuario.SenhaHash = _hasher.HashPassword(usuario, dto.Senha);

        var adulto = new Adulto
        {
            Usuario = usuario,
            Cpf = dto.Cpf,
            Telefone = dto.Telefone,
        };

        var conta = new Conta
        {
            Usuario = usuario,
            Saldo = 0,
            AtualizadaEm = agora,
        };

        _db.Adultos.Add(adulto);
        _db.Contas.Add(conta);
        await _db.SaveChangesAsync();

        return ParaDto(usuario);
    }

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
        var usuario = await _db.Usuarios.FindAsync(User.UsuarioId());

        // Cookie ainda válido, mas o usuário foi desativado ou apagado depois do login
        if (usuario is null || !usuario.Ativo)
            return Unauthorized();

        return ParaDto(usuario);
    }

    /// <summary>
    /// Compara a senha digitada com o hash salvo no banco.
    /// </summary>
    private bool SenhaConfere(Usuario usuario, string senha)
    {
        var resultado = _hasher.VerifyHashedPassword(usuario, usuario.SenhaHash, senha);

        return resultado != PasswordVerificationResult.Failed;
    }

    /// <summary>
    /// Grava no cookie quem é o usuário (Id, nome e permissão).
    /// A permissão vira Role, o que habilita [Authorize(Roles = "Admin")] nas outras rotas.
    /// </summary>
    private async Task CriarSessao(Usuario usuario)
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

    /// <summary>
    /// Converte a entidade para o que a API devolve (sem o hash da senha).
    /// </summary>
    private static UsuarioLogadoDto ParaDto(Usuario usuario) => new()
    {
        Id = usuario.Id,
        Nome = usuario.Nome,
        Email = usuario.Email,
        Permissao = usuario.Permissao,
    };
}
