using Backend.Data;
using Backend.DTOs.Painel;
using Backend.DTOs.Pedidos;
using Backend.Mappings;
using Backend.Models;
using Backend.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

// Painel da cantina (cena 2). Tudo aqui é só do Admin.
[ApiController]
[Route("api/painel")]
[Authorize(Roles = "Admin")]
public class PainelController : ControllerBase
{
    private readonly AppDbContext _db;

    public PainelController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Pedidos antecipados do próximo intervalo de hoje (o que está acontecendo ou o próximo a começar).
    /// </summary>
    [HttpGet("intervalo-atual")]
    public async Task<PainelDto> IntervaloAtual()
    {
        var intervalo = await ProximoIntervalo();
        if (intervalo == null)
            return new PainelDto();

        var pedidos = await PedidosDoIntervalo(intervalo);

        return new PainelDto
        {
            Intervalo = intervalo.Nome,
            HoraInicio = intervalo.HoraInicio,
            HoraFim = intervalo.HoraFim,
            Pendentes = pedidos.Count(p => p.Status != StatusPedido.Entregue),
            Entregues = pedidos.Count(p => p.Status == StatusPedido.Entregue),
            Pedidos = pedidos.Select(p => p.ParaDto()).ToList(),
        };
    }

    /// <summary>
    /// Consolidado por item dos pedidos ainda não entregues (ex.: "12x Pão de queijo") para a cozinha.
    /// </summary>
    [HttpGet("preparo")]
    public async Task<List<PreparoItemDto>> Preparo()
    {
        var intervalo = await ProximoIntervalo();
        if (intervalo == null)
            return [];

        var pedidos = await PedidosDoIntervalo(intervalo);

        return pedidos
            .Where(p => p.Status != StatusPedido.Entregue)
            .SelectMany(p => p.Itens)
            .GroupBy(ip => ip.Item.Nome)
            .Select(g => new PreparoItemDto { Nome = g.Key, Quantidade = g.Sum(ip => ip.Quantidade) })
            .OrderByDescending(i => i.Quantidade)
            .ToList();
    }

    /// <summary>
    /// Procura entre os pedidos antecipados de hoje por código de retirada, nome ou e-mail (R4).
    /// </summary>
    [HttpGet("buscar")]
    public async Task<List<PedidoDto>> Buscar(string termo)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Now);

        var pedidos = await _db.Pedidos.ComDetalhes()
            .Where(p => p.Data == hoje &&
                        p.TipoVenda == TipoVenda.Antecipado &&
                        p.Status != StatusPedido.Cancelado &&
                        (p.CodigoRetirada == termo ||
                         p.Usuario.Nome.Contains(termo) ||
                         p.Usuario.Email.Contains(termo)))
            .ToListAsync();

        return pedidos.Select(p => p.ParaDto()).ToList();
    }

    // Intervalo de hoje que ainda não terminou, o mais cedo primeiro
    private async Task<Intervalo?> ProximoIntervalo()
    {
        var agora = TimeOnly.FromDateTime(DateTime.Now);

        return await _db.Intervalos
            .Where(i => i.HoraFim >= agora)
            .OrderBy(i => i.HoraInicio)
            .FirstOrDefaultAsync();
    }

    // Pedidos antecipados (não cancelados) de hoje neste intervalo, na ordem em que foram feitos
    private async Task<List<Pedido>> PedidosDoIntervalo(Intervalo intervalo)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Now);

        return await _db.Pedidos.ComDetalhes()
            .Where(p => p.Data == hoje &&
                        p.IntervaloId == intervalo.Id &&
                        p.TipoVenda == TipoVenda.Antecipado &&
                        p.Status != StatusPedido.Cancelado)
            .OrderBy(p => p.CriadoEm)
            .ToListAsync();
    }
}
