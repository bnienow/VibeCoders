using System.Security.Claims;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Extensions;

public static class AcessoExtensions
{
    /// <summary>
    /// Aluno que o usuário logado pode ver: ele mesmo, o responsável dele ou o Admin.
    /// Devolve nulo se não existe ou se não pode ver (a rota responde 404 nos dois casos).
    /// </summary>
    public static async Task<Aluno?> BuscarAlunoVisivel(this AppDbContext db, ClaimsPrincipal user, int alunoId)
    {
        var aluno = await db.Alunos
            .Include(a => a.Usuario).ThenInclude(u => u.Conta)
            .FirstOrDefaultAsync(a => a.UsuarioId == alunoId);

        if (aluno == null)
            return null;

        var eu = user.UsuarioId();
        var podeVer = user.IsInRole("Admin") || aluno.UsuarioId == eu || aluno.AdultoId == eu;

        return podeVer ? aluno : null;
    }
}
