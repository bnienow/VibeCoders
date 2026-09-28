using Backend.Data.Repositories;
using Backend.Data.Services;
using Backend.Domain.Interfaces.Repositories;
using Backend.Domain.Interfaces.Services;

namespace Backend.Api.Extensions;

public static class InjecaoDependenciaExtensions
{
    /// <summary>
    /// Acesso ao banco. Scoped: todos compartilham o mesmo AppDbContext da requisição,
    /// então o IUnitOfWork grava de uma vez o que qualquer repositório alterou.
    /// </summary>
    public static IServiceCollection AddRepositorios(this IServiceCollection services) =>
        services
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped<IUsuarioRepository, UsuarioRepository>()
            .AddScoped<IAlunoRepository, AlunoRepository>()
            .AddScoped<IAdultoRepository, AdultoRepository>()
            .AddScoped<IContaRepository, ContaRepository>()
            .AddScoped<IMetodoPagamentoRepository, MetodoPagamentoRepository>()
            .AddScoped<IItemRepository, ItemRepository>()
            .AddScoped<IIntervaloRepository, IntervaloRepository>()
            .AddScoped<IPedidoRepository, PedidoRepository>()
            .AddScoped<IFechamentoRepository, FechamentoRepository>();

    /// <summary>
    /// Regras de negócio, uma por área. Os controllers só dependem destas interfaces.
    /// </summary>
    public static IServiceCollection AddServicos(this IServiceCollection services) =>
        services
            .AddScoped<IAuthService, AuthService>()
            .AddScoped<IContaService, ContaService>()
            .AddScoped<IAlunoService, AlunoService>()
            .AddScoped<IExtratoService, ExtratoService>()
            .AddScoped<IFechamentoService, FechamentoService>()
            .AddScoped<IIntervaloService, IntervaloService>()
            .AddScoped<IItemService, ItemService>()
            .AddScoped<IPagamentoService, PagamentoService>()
            .AddScoped<IPainelService, PainelService>()
            .AddScoped<IPedidoService, PedidoService>()
            .AddScoped<IRelatorioService, RelatorioService>();
}
