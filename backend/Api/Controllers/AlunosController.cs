using Backend.Api.Extensions;
using Backend.Domain.DTOs.Alunos;
using Backend.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/alunos")]
public class AlunosController : ControllerBase
{
    private readonly IAlunoService _alunos;

    public AlunosController(IAlunoService alunos)
    {
        _alunos = alunos;
    }

    /// <summary>
    /// Busca alunos por nome ou e-mail (campo de busca do balcão): até 20 resultados.
    /// Sem busca, devolve todos (o balcão guarda essa lista para funcionar sem conexão).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public Task<List<AlunoDto>> Buscar(string? busca) => _alunos.Buscar(busca);

    /// <summary>
    /// Detalhe com saldo e restrições. Veem: o próprio aluno, o responsável e o Admin.
    /// </summary>
    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<AlunoDto>> Detalhe(int id) =>
        OuNaoEncontrado(await _alunos.Detalhe(id, User.UsuarioId(), User.IsInRole("Admin")));

    /// <summary>
    /// O responsável logado cadastra um filho, já vinculado a ele, com conta de saldo zero (R1).
    /// </summary>
    [Authorize(Roles = "Adulto")]
    [HttpPost]
    public Task<AlunoDto> Cadastrar(CreateAlunoDto dto) => _alunos.Cadastrar(User.UsuarioId(), dto);

    /// <summary>
    /// Filhos do responsável (dashboard). Só o próprio responsável ou o Admin.
    /// </summary>
    [Authorize]
    [HttpGet("/api/adultos/{adultoId}/filhos")]
    public async Task<ActionResult<List<AlunoDto>>> Filhos(int adultoId) =>
        OuNaoEncontrado(await _alunos.Filhos(adultoId, User.UsuarioId(), User.IsInRole("Admin")));

    /// <summary>
    /// Define o limite de gasto diário do filho (D2). Só o responsável; o aluno não altera o próprio limite.
    /// </summary>
    [Authorize(Roles = "Adulto")]
    [HttpPut("{id}/limite")]
    public async Task<ActionResult<AlunoDto>> DefinirLimite(int id, LimiteDto dto) =>
        OuNaoEncontrado(await _alunos.DefinirLimite(id, User.UsuarioId(), dto.LimiteDiario));

    /// <summary>
    /// Define as restrições alimentares do filho (D3), ex.: ["Lactose"].
    /// </summary>
    [Authorize(Roles = "Adulto")]
    [HttpPut("{id}/restricoes")]
    public async Task<ActionResult<AlunoDto>> DefinirRestricoes(int id, RestricoesDto dto) =>
        OuNaoEncontrado(await _alunos.DefinirRestricoes(id, User.UsuarioId(), dto.Restricoes));

    /// <summary>
    /// Responsável coloca crédito na conta do filho (pagamento simulado, D1).
    /// </summary>
    [Authorize(Roles = "Adulto")]
    [HttpPost("{id}/credito")]
    public async Task<ActionResult<AlunoDto>> AdicionarCredito(int id, CreditoDto dto) =>
        OuNaoEncontrado(await _alunos.AdicionarCredito(id, User.UsuarioId(), dto));

    // Nulo = não existe ou o usuário não pode ver: 404 nos dois casos
    private ActionResult<T> OuNaoEncontrado<T>(T? resultado) where T : class =>
        resultado == null ? NotFound() : resultado;
}
