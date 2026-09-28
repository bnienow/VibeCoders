using Backend.Domain.DTOs.Auth;
using Backend.Domain.Exceptions;
using Backend.Domain.Interfaces.Repositories;
using Backend.Domain.Interfaces.Services;
using Backend.Domain.Models;
using Backend.Domain.Models.Enums;
using Microsoft.AspNetCore.Identity;

namespace Backend.Data.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IAdultoRepository _adultos;
    private readonly IContaRepository _contas;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher<Usuario> _hasher;

    public AuthService(IUsuarioRepository usuarios, IAdultoRepository adultos, IContaRepository contas,
        IUnitOfWork uow, IPasswordHasher<Usuario> hasher)
    {
        _usuarios = usuarios;
        _adultos = adultos;
        _contas = contas;
        _uow = uow;
        _hasher = hasher;
    }

    public async Task<UsuarioLogadoDto?> Login(LoginDto dto)
    {
        var usuario = await _usuarios.BuscarAtivoPorEmail(NormalizarEmail(dto.Email));

        if (usuario is null || !SenhaConfere(usuario, dto.Senha))
            return null;

        return ParaDto(usuario);
    }

    public async Task<UsuarioLogadoDto> Registrar(RegistroDto dto)
    {
        var email = NormalizarEmail(dto.Email);
        if (await _usuarios.EmailEmUso(email))
            throw new ConflitoException("E-mail já cadastrado");

        var agora = DateTime.Now;

        var usuario = new Usuario
        {
            Nome = dto.Nome.Trim(),
            Email = email,
            Permissao = Permissao.Adulto,
            CriadoEm = agora,
        };
        usuario.SenhaHash = _hasher.HashPassword(usuario, dto.Senha);

        _adultos.Adicionar(new Adulto { Usuario = usuario, Cpf = dto.Cpf, Telefone = dto.Telefone });
        _contas.Adicionar(new Conta { Usuario = usuario, Saldo = 0, AtualizadaEm = agora });
        await _uow.SalvarAsync();

        return ParaDto(usuario);
    }

    public async Task<UsuarioLogadoDto?> BuscarAtivo(int id)
    {
        var usuario = await _usuarios.BuscarPorId(id);

        return usuario is { Ativo: true } ? ParaDto(usuario) : null;
    }

    private static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();

    private bool SenhaConfere(Usuario usuario, string senha) =>
        _hasher.VerifyHashedPassword(usuario, usuario.SenhaHash, senha) != PasswordVerificationResult.Failed;

    // Sem o hash da senha
    private static UsuarioLogadoDto ParaDto(Usuario usuario) => new()
    {
        Id = usuario.Id,
        Nome = usuario.Nome,
        Email = usuario.Email,
        Permissao = usuario.Permissao,
    };
}
