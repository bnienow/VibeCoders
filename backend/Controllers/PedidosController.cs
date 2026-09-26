using Backend.Data;
using Backend.DTOs.Pedidos;
using Backend.Extensions;
using Backend.Mappings;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

// As regras ficam no PedidoService; aqui só recebe a requisição e devolve o resultado.
// Regra violada vira RegraException, que o Program.cs responde como 400 com a mensagem.
[ApiController]
[Route("api/pedidos")]
public class PedidosController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly PedidoService _pedidos;

    public PedidosController(AppDbContext db, PedidoService pedidos)
    {
        _db = db;
        _pedidos = pedidos;
    }

    /// <summary>
    /// Pedido antecipado do aluno ou adulto logado (cena 1).
    /// </summary>
    [Authorize(Roles = "Aluno,Adulto")]
    [HttpPost]
    public async Task<PedidoDto> Criar(CriarPedidoDto dto)
    {
        var pedido = await _pedidos.CriarAntecipado(User.UsuarioId(), dto);

        return await BuscarDto(pedido.Id);
    }

    /// <summary>
    /// Venda no balcão registrada pelo Admin (cena 3). Nasce Entregue, não entra na fila do painel.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("balcao")]
    public async Task<PedidoDto> VenderNoBalcao(VendaBalcaoDto dto)
    {
        var pedido = await _pedidos.VenderNoBalcao(dto);

        return await BuscarDto(pedido.Id);
    }

    /// <summary>
    /// Reenvia as vendas de balcão feitas sem internet (D6) e diz quais entraram.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("sincronizar")]
    public async Task<List<ResultadoSincronizacaoDto>> Sincronizar(List<VendaBalcaoDto> vendas)
    {
        return await _pedidos.Sincronizar(vendas);
    }

    /// <summary>
    /// Pedidos do usuário logado, do mais recente para o mais antigo.
    /// </summary>
    [Authorize]
    [HttpGet("meus")]
    public async Task<List<PedidoDto>> Meus()
    {
        var pedidos = await _db.Pedidos.ComDetalhes()
            .Where(p => p.UsuarioId == User.UsuarioId())
            .OrderByDescending(p => p.CriadoEm)
            .ToListAsync();

        return pedidos.Select(p => p.ParaDto()).ToList();
    }

    /// <summary>
    /// Um pedido. Só o dono ou o Admin veem.
    /// </summary>
    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<PedidoDto>> Detalhe(int id)
    {
        var pedido = await _db.Pedidos.ComDetalhes().FirstOrDefaultAsync(p => p.Id == id);

        if (pedido == null || (pedido.UsuarioId != User.UsuarioId() && !User.IsInRole("Admin")))
            return NotFound();

        return pedido.ParaDto();
    }

    /// <summary>
    /// Troca os itens do pedido, enquanto a janela estiver aberta.
    /// </summary>
    [Authorize(Roles = "Aluno,Adulto")]
    [HttpPut("{id}")]
    public async Task<PedidoDto> Alterar(int id, AlterarPedidoDto dto)
    {
        await _pedidos.Alterar(id, User.UsuarioId(), dto);

        return await BuscarDto(id);
    }

    /// <summary>
    /// Cancela o pedido e estorna o valor, enquanto a janela estiver aberta.
    /// </summary>
    [Authorize(Roles = "Aluno,Adulto")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancelar(int id)
    {
        await _pedidos.Cancelar(id, User.UsuarioId());

        return NoContent();
    }

    /// <summary>
    /// Marca o pedido como entregue (painel da cantina, cena 2).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/entregar")]
    public async Task<IActionResult> Entregar(int id)
    {
        await _pedidos.Entregar(id);

        return NoContent();
    }

    // Relê o pedido do banco já com os detalhes, para devolver depois de gravar
    private async Task<PedidoDto> BuscarDto(int id) =>
        (await _db.Pedidos.ComDetalhes().SingleAsync(p => p.Id == id)).ParaDto();
}
