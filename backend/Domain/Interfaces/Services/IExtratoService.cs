using Backend.Domain.DTOs.Extratos;

namespace Backend.Domain.Interfaces.Services;

public interface IExtratoService
{
    // Movimentos do mês ("2026-08"; nulo = mês atual). Nulo se o aluno não existe ou o usuário não pode ver
    Task<ExtratoDto?> DoAluno(int alunoId, string? mes, int usuarioId, bool ehAdmin);
}
