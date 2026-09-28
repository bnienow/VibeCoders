namespace Backend.Domain.Interfaces.Repositories;

/// <summary>
/// Grava de uma vez tudo o que os repositórios alteraram na requisição.
/// Um SaveChanges só = uma transação: ou grava tudo, ou nada.
/// </summary>
public interface IUnitOfWork
{
    Task SalvarAsync();

    // Descarta o que ficou pela metade na memória (ex.: venda offline rejeitada no meio)
    void DescartarAlteracoes();
}
