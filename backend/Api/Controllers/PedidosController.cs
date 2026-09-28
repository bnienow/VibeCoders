using Backend.Api.Extensions;
using Backend.Domain.DTOs.Pedidos;
using Backend.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

// As regras ficam no PedidoService; aqui só recebe a requisição e devolve o resultado.
// Regra violada vira RegraException, que o Program.cs responde como 400 com a mensagem.
[ApiController]
[Route("api/pedidos")]
public class PedidosController : ControllerBase
{
    private readonly IPedidoService _pedidos;

    public PedidosController(IPedidoService pedidos)
    {
        _pedidos = pedidos;
    }

    /// <summary>
    /// Pedido antecipado do aluno ou adulto logado (cena 1).
    /// </summary>
    [Authorize(Roles = "Aluno,Adulto")]
    [HttpPost]
    public Task<PedidoDto> Criar(CriarPedidoDto dto) => _pedidos.CriarAntecipado(User.UsuarioId(), dto);

    /// <summary>
    /// Venda no balcão registrada pelo Admin (cena 3). Nasce Entregue, não entra na fila do painel.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("balcao")]
    public Task<PedidoDto> VenderNoBalcao(VendaBalcaoDto dto) => _pedidos.VenderNoBalcao(dto);

    /// <summary>
    /// Reenvia as vendas de balcão feitas sem internet (D6) e diz quais entraram.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("sincronizar")]
    public Task<List<ResultadoSincronizacaoDto>> Sincronizar(List<VendaBalcaoDto> vendas) =>
        _pedidos.Sincronizar(vendas);

    /// <summary>
    /// Pedidos do usuário logado, do mais recente para o mais antigo.
    /// </summary>
    [Authorize]
    [HttpGet("meus")]
    public Task<List<PedidoDto>> Meus() => _pedidos.DoUsuario(User.UsuarioId());

    /// <summary>
    /// Um pedido. Só o dono ou o Admin veem.
    /// </summary>
    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<PedidoDto>> Detalhe(int id)
    {
        var pedido = await _pedidos.Detalhe(id, User.UsuarioId(), User.IsInRole("Admin"));
        if (pedido == null)
            return NotFound();

        return pedido;
    }

    /// <summary>
    /// Troca os itens do pedido, enquanto a janela estiver aberta.
    /// </summary>
    [Authorize(Roles = "Aluno,Adulto")]
    [HttpPut("{id}")]
    public Task<PedidoDto> Alterar(int id, AlterarPedidoDto dto) => _pedidos.Alterar(id, User.UsuarioId(), dto);

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
}
