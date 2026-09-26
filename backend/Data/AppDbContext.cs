using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Aluno> Alunos => Set<Aluno>();
    public DbSet<Adulto> Adultos => Set<Adulto>();
    public DbSet<Conta> Contas => Set<Conta>();
    public DbSet<Item> Itens => Set<Item>();
    public DbSet<DispCardapio> DispCardapios => Set<DispCardapio>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<ItemPedido> ItensPedido => Set<ItemPedido>();
    public DbSet<Movimento> Movimentos => Set<Movimento>();
    public DbSet<Intervalo> Intervalos => Set<Intervalo>();
    public DbSet<MetodoPagamento> MetodosPagamento => Set<MetodoPagamento>();
    public DbSet<Fechamento> Fechamentos => Set<Fechamento>();

    // Regras globais: dinheiro com 2 casas e texto limitado (MySQL não indexa longtext)
    protected override void ConfigureConventions(ModelConfigurationBuilder b)
    {
        b.Properties<decimal>().HavePrecision(10, 2);
        b.Properties<string>().HaveMaxLength(255);
    }

    // Só o que a convenção de nomes do EF não descobre sozinha
    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Usuario>().HasIndex(u => u.Email).IsUnique();

        mb.Entity<Aluno>().HasKey(a => a.UsuarioId);
        mb.Entity<Aluno>()
            .HasOne(a => a.Adulto).WithMany(ad => ad.Filhos)
            .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<Adulto>().HasKey(a => a.UsuarioId);
        mb.Entity<Adulto>()
            .HasOne(a => a.MetodoPagamentoPadrao).WithMany()
            .HasForeignKey(a => a.MetodoPagamentoPadraoId)
            .OnDelete(DeleteBehavior.SetNull);

        mb.Entity<ItemPedido>().HasKey(ip => new { ip.PedidoId, ip.ItemId });
        mb.Entity<DispCardapio>().HasKey(d => new { d.Data, d.IntervaloId, d.ItemId });
        mb.Entity<Pedido>().HasIndex(p => new { p.Data, p.CodigoRetirada }).IsUnique();

        // Todo enum é gravado pelo nome ("Entregue"), não pelo número
        foreach (var prop in mb.Model.GetEntityTypes()
                     .SelectMany(t => t.GetProperties())
                     .Where(p => p.ClrType.IsEnum))
        {
            prop.SetProviderClrType(typeof(string));
            prop.SetMaxLength(20);
        }
    }
}
