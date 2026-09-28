using Backend.Domain.DTOs.Alunos;
using Backend.Domain.Exceptions;
using Backend.Domain.Helpers;
using Backend.Domain.Interfaces.Repositories;
using Backend.Domain.Interfaces.Services;
using Backend.Domain.Models;
using Backend.Domain.Models.Enums;
using Microsoft.AspNetCore.Identity;

namespace Backend.Data.Services;

public class AlunoService : IAlunoService
{
    private const int LimiteBusca = 20;

    private readonly IAlunoRepository _alunos;
    private readonly IUsuarioRepository _usuarios;
    private readonly IContaRepository _contas;
    private readonly IContaService _conta;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher<Usuario> _hasher;

    public AlunoService(IAlunoRepository alunos, IUsuarioRepository usuarios, IContaRepository contas,
        IContaService conta, IUnitOfWork uow, IPasswordHasher<Usuario> hasher)
    {
        _alunos = alunos;
        _usuarios = usuarios;
        _contas = contas;
        _conta = conta;
        _uow = uow;
        _hasher = hasher;
    }

    public async Task<List<AlunoDto>> Buscar(string? busca)
    {
        var alunos = await _alunos.Buscar(busca, busca == null ? int.MaxValue : LimiteBusca);

        return await ParaDtos(alunos);
    }

    public async Task<AlunoDto?> Detalhe(int id, int usuarioId, bool ehAdmin)
    {
        var aluno = await _alunos.BuscarComConta(id);
        if (aluno == null || !aluno.VisivelPara(usuarioId, ehAdmin))
            return null;

        return await ParaDto(aluno);
    }

    public async Task<AlunoDto> Cadastrar(int adultoId, CreateAlunoDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        if (!email.EndsWith("@" + Aluno.DominioEmail))
            throw new RegraException($"O e-mail do aluno precisa ser @{Aluno.DominioEmail}");

        if (await _usuarios.EmailEmUso(email))
            throw new RegraException("E-mail já cadastrado");

        var agora = DateTime.Now;

        var usuario = new Usuario
        {
            Nome = dto.Nome.Trim(),
            Email = email,
            Permissao = Permissao.Aluno,
            CriadoEm = agora,
        };
        usuario.SenhaHash = _hasher.HashPassword(usuario, dto.Senha);

        var aluno = new Aluno
        {
            Usuario = usuario,
            AdultoId = adultoId,
            Turma = dto.Turma,
            DataNascimento = dto.DataNascimento,
            RestricoesAlimentares = Alergia.ParaTexto(dto.Restricoes),
        };

        _alunos.Adicionar(aluno);
        _contas.Adicionar(new Conta { Usuario = usuario, Saldo = 0, AtualizadaEm = agora });
        await _uow.SalvarAsync();

        return await ParaDto(aluno);
    }

    public async Task<List<AlunoDto>?> Filhos(int adultoId, int usuarioId, bool ehAdmin)
    {
        if (adultoId != usuarioId && !ehAdmin)
            return null;

        return await ParaDtos(await _alunos.FilhosDe(adultoId));
    }

    public async Task<AlunoDto?> DefinirLimite(int alunoId, int adultoId, decimal? limiteDiario)
    {
        var aluno = await _alunos.BuscarFilho(alunoId, adultoId);
        if (aluno == null)
            return null;

        aluno.LimiteDiario = limiteDiario;
        await _uow.SalvarAsync();

        return await ParaDto(aluno);
    }

    public async Task<AlunoDto?> DefinirRestricoes(int alunoId, int adultoId, string[] restricoes)
    {
        var aluno = await _alunos.BuscarFilho(alunoId, adultoId);
        if (aluno == null)
            return null;

        aluno.RestricoesAlimentares = Alergia.ParaTexto(restricoes);
        await _uow.SalvarAsync();

        return await ParaDto(aluno);
    }

    public async Task<AlunoDto?> AdicionarCredito(int alunoId, int adultoId, CreditoDto dto)
    {
        var aluno = await _alunos.BuscarFilho(alunoId, adultoId);
        if (aluno == null)
            return null;

        await _conta.AdicionarCredito(adultoId, alunoId, dto.Valor, dto.MetodoPagamentoId);
        await _uow.SalvarAsync();

        return await ParaDto(aluno);
    }

    private async Task<AlunoDto> ParaDto(Aluno aluno) => (await ParaDtos([aluno])).Single();

    // Converte para DTO, calculando o gasto do mês de todos numa consulta só
    private async Task<List<AlunoDto>> ParaDtos(List<Aluno> alunos)
    {
        var hoje = DateTime.Today;
        var gastos = await _contas.GastosDesde(
            alunos.Select(a => a.UsuarioId).ToList(),
            new DateTime(hoje.Year, hoje.Month, 1));

        return alunos.Select(a => new AlunoDto
        {
            Id = a.UsuarioId,
            Nome = a.Usuario.Nome,
            Email = a.Usuario.Email,
            Turma = a.Turma,
            DataNascimento = a.DataNascimento,
            AdultoId = a.AdultoId,
            Saldo = a.Usuario.Conta?.Saldo ?? 0,
            LimiteDiario = a.LimiteDiario,
            Restricoes = Alergia.ParaLista(a.RestricoesAlimentares),
            GastoMes = gastos.GetValueOrDefault(a.UsuarioId),
        }).ToList();
    }
}
