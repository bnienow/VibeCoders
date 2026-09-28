using Backend.Domain.Models;

namespace Backend.Domain.Interfaces.Repositories;

public interface IAlunoRepository
{
    // Com usuário e conta carregados (nome, e-mail e saldo)
    Task<Aluno?> BuscarComConta(int id);

    // Filho do responsável; nulo se não for filho dele
    Task<Aluno?> BuscarFilho(int id, int adultoId);

    // Ativos por nome ou e-mail, em ordem alfabética; sem busca, todos
    Task<List<Aluno>> Buscar(string? busca, int limite);

    Task<List<Aluno>> FilhosDe(int adultoId);

    // Sem includes: só os dados do aluno (limite diário, restrições)
    Task<Aluno?> BuscarPorId(int id);

    Task<bool> EhFilho(int alunoId, int adultoId);

    void Adicionar(Aluno aluno);
}
