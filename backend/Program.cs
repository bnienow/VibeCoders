using System.Text.Json.Serialization;
using Backend.Data;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// QuestPDF (PDF do extrato) é gratuito para projetos pequenos, mas exige declarar a licença
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// ---------- Serviços ----------

// Conexão com o banco
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 36))));

// Hash de senha (usado no seed e no login)
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

// Regras de negócio: conta corrente e pedidos (janela, estoque, limites)
builder.Services.AddScoped<ContaService>();
builder.Services.AddScoped<PedidoService>();
builder.Services.AddScoped<FechamentoService>();

// Controllers; a API envia e recebe os enums pelo nome ("Entregue"), não pelo número
builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Login por cookie. Por padrão o cookie redireciona pra uma página de login;
// numa API isso não faz sentido, então responde só o status e o front decide o que mostrar.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        // Não logado
        o.Events.OnRedirectToLogin = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };

        // Logado, mas sem permissão
        o.Events.OnRedirectToAccessDenied = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

var app = builder.Build();

// ---------- Inicialização ----------

// Massa de teste: só popula se o banco estiver vazio
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Usuario>>();

    SeedData.Popular(db, hasher);
}

// ---------- Pipeline HTTP ----------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Ordem importa: primeiro descobre quem é (cookie), depois checa se pode
app.UseAuthentication();
app.UseAuthorization();

// Erros esperados viram resposta com mensagem, que o front mostra na tela:
// regra violada → 400; conflito de concorrência → 409
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (RegraException e)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync(e.Message);
    }
    catch (DbUpdateConcurrencyException)
    {
        // Outra operação mudou o mesmo estoque ou saldo entre a leitura e a gravação: nada foi gravado
        context.Response.StatusCode = StatusCodes.Status409Conflict;
        await context.Response.WriteAsync("Outra operação mudou o estoque ou o saldo ao mesmo tempo. Tente de novo.");
    }
});

app.MapControllers();

app.Run();
