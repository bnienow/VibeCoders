using Backend.Data;
using Backend.DTOs.Alunos;
using Backend.Extensions;
using Backend.Models;
using Backend.Models.Enums;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/alunos")]
public class AlunosController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ContaService _conta;
    private readonly IPasswordHasher<Usuario> _hasher;

    public AlunosController(AppDbContext db, ContaService conta, IPasswordHasher<Usuario> hasher)
    {
        _db = db;
        _conta = conta;
        _hasher = hasher;
    }

    /// <summary>
    /// Busca alunos por nome ou e-mail (campo de busca do balcão). Até 20 resultados.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<List<AlunoDto>> Buscar(string? busca)
    {
        var alunos = await ComConta()
            .Where(a => a.Usuario.Ativo &&
                        (busca == null || a.Usuario.Nome.Contains(busca) || a.Usuario.Email.Contains(busca)))
            .OrderBy(a => a.Usuario.Nome)
            .Take(20)
            .ToListAsync();

        return await ParaDtos(alunos);
    }

    /// <summary>
    /// Detalhe com saldo e restrições. Veem: o próprio aluno, o responsável e o Admin.
    /// </summary>
    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<AlunoDto>> Detalhe(int id)
    {
        var aluno = await _db.BuscarAlunoVisivel(User, id);
        if (aluno == null)
            return NotFound();

        return (await ParaDtos([aluno])).Single();
    }

    /// <summary>
    /// O responsável logado cadastra um filho, já vinculado a ele, com conta de saldo zero (R1).
    /// </summary>
    [Authorize(Roles = "Adulto")]
    [HttpPost]
    public async Task<AlunoDto> Cadastrar(CreateAlunoDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        if (!email.EndsWith("@" + Aluno.DominioEmail))
            throw new RegraException($"O e-mail do aluno precisa ser @{Aluno.DominioEmail}");

        if (await _db.Usuarios.AnyAsync(u => u.Email == email))
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
            AdultoId = User.UsuarioId(),
            Turma = dto.Turma,
            DataNascimento = dto.DataNascimento,
            RestricoesAlimentares = Alergia.ParaTexto(dto.Restricoes),
        };

        _db.Alunos.Add(aluno);
        _db.Contas.Add(new Conta { Usuario = usuario, Saldo = 0, AtualizadaEm = agora });
        await _db.SaveChangesAsync();

        return (await ParaDtos([aluno])).Single();
    }

    /// <summary>
    /// Filhos do responsável (dashboard). Só o próprio responsável ou o Admin.
    /// </summary>
    [Authorize]
    [HttpGet("/api/adultos/{adultoId}/filhos")]
    public async Task<ActionResult<List<AlunoDto>>> Filhos(int adultoId)
    {
        if (adultoId != User.UsuarioId() && !User.IsInRole("Admin"))
            return NotFound();

        var filhos = await ComConta()
            .Where(a => a.AdultoId == adultoId)
            .OrderBy(a => a.Usuario.Nome)
            .ToListAsync();

        return await ParaDtos(filhos);
    }

    /// <summary>
    /// Define o limite de gasto diário do filho (D2). Só o responsável; o aluno não altera o próprio limite.
    /// </summary>
    [Authorize(Roles = "Adulto")]
    [HttpPut("{id}/limite")]
    public async Task<ActionResult<AlunoDto>> DefinirLimite(int id, LimiteDto dto)
    {
        var aluno = await BuscarFilho(id);
        if (aluno == null)
            return NotFound();

        aluno.LimiteDiario = dto.LimiteDiario;
        await _db.SaveChangesAsync();

        return (await ParaDtos([aluno])).Single();
    }

    /// <summary>
    /// Define as restrições alimentares do filho (D3), ex.: ["Lactose"].
    /// </summary>
    [Authorize(Roles = "Adulto")]
    [HttpPut("{id}/restricoes")]
    public async Task<ActionResult<AlunoDto>> DefinirRestricoes(int id, RestricoesDto dto)
    {
        var aluno = await BuscarFilho(id);
        if (aluno == null)
            return NotFound();

        aluno.RestricoesAlimentares = Alergia.ParaTexto(dto.Restricoes);
        await _db.SaveChangesAsync();

        return (await ParaDtos([aluno])).Single();
    }

    /// <summary>
    /// Responsável coloca crédito na conta do filho (pagamento simulado, D1).
    /// </summary>
    [Authorize(Roles = "Adulto")]
    [HttpPost("{id}/credito")]
    public async Task<ActionResult<AlunoDto>> AdicionarCredito(int id, CreditoDto dto)
    {
        var aluno = await BuscarFilho(id);
        if (aluno == null)
            return NotFound();

        await _conta.AdicionarCredito(User.UsuarioId(), id, dto.Valor, dto.MetodoPagamentoId);
        await _db.SaveChangesAsync();

        return (await ParaDtos([aluno])).Single();
    }

    // Alunos com usuário e conta carregados (nome, e-mail e saldo)
    private IQueryable<Aluno> ComConta() =>
        _db.Alunos.Include(a => a.Usuario).ThenInclude(u => u.Conta);

    // Filho do responsável logado; nulo se não for filho dele
    private Task<Aluno?> BuscarFilho(int id) =>
        ComConta().FirstOrDefaultAsync(a => a.UsuarioId == id && a.AdultoId == User.UsuarioId());

    // Converte para DTO, calculando o gasto do mês de todos numa consulta só
    private async Task<List<AlunoDto>> ParaDtos(List<Aluno> alunos)
    {
        var ids = alunos.Select(a => a.UsuarioId).ToList();
        var hoje = DateTime.Today;
        var inicioDoMes = new DateTime(hoje.Year, hoje.Month, 1);

        // Compras são negativas e estornos positivos: a soma invertida é o gasto líquido
        var gastos = await _db.Movimentos
            .Where(m => ids.Contains(m.Conta.UsuarioId) &&
                        m.Data >= inicioDoMes &&
                        (m.Tipo == TipoMovimento.Compra || m.Tipo == TipoMovimento.Estorno))
            .GroupBy(m => m.Conta.UsuarioId)
            .Select(g => new { UsuarioId = g.Key, Gasto = -g.Sum(m => m.Valor) })
            .ToDictionaryAsync(g => g.UsuarioId, g => g.Gasto);

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
