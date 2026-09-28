using Backend.Data;
using Backend.Domain.Interfaces.Repositories;

namespace Backend.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;

    public UnitOfWork(AppDbContext db)
    {
        _db = db;
    }

    public Task SalvarAsync() => _db.SaveChangesAsync();

    public void DescartarAlteracoes() => _db.ChangeTracker.Clear();
}
