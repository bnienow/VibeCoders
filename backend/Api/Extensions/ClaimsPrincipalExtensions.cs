using System.Security.Claims;

namespace Backend.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Id do usuário logado, lido do cookie de sessão.
    /// Só use em rotas com [Authorize]: sem sessão não há Id.
    /// </summary>
    public static int UsuarioId(this ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
