using Backend.Domain.DTOs.Alunos;

namespace Backend.Domain.Interfaces.Services;

/// <summary>
/// Alunos e o que o responsável controla neles. Métodos que devolvem nulo = não existe ou o usuário não pode ver (a rota responde 404).
/// </summary>
public interface IAlunoService
{
    // Busca por nome ou e-mail (até 20); sem busca, todos os ativos (o balcão guarda para usar offline)
    Task<List<AlunoDto>> Buscar(string? busca);

    Task<AlunoDto?> Detalhe(int id, int usuarioId, bool ehAdmin);

    // O responsável cadastra um filho, já vinculado a ele, com conta de saldo zero (R1)
    Task<AlunoDto> Cadastrar(int adultoId, CreateAlunoDto dto);

    // Só o próprio responsável ou o Admin
    Task<List<AlunoDto>?> Filhos(int adultoId, int usuarioId, bool ehAdmin);

    Task<AlunoDto?> DefinirLimite(int alunoId, int adultoId, decimal? limiteDiario);
    Task<AlunoDto?> DefinirRestricoes(int alunoId, int adultoId, string[] restricoes);
    Task<AlunoDto?> AdicionarCredito(int alunoId, int adultoId, CreditoDto dto);
}
