using Backend.Api.Extensions;
using Backend.Domain.DTOs.Pagamentos;
using Backend.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

// Métodos de pagamento e crédito simulado (D1). Nada aqui fala com gateway real: todo pagamento é aprovado.
[ApiController]
[Route("api/pagamentos")]
[Authorize(Roles = "Adulto")]
public class PagamentosController : ControllerBase
{
    private readonly IPagamentoService _pagamentos;

    public PagamentosController(IPagamentoService pagamentos)
    {
        _pagamentos = pagamentos;
    }

    /// <summary>
    /// Métodos ativos do responsável logado, indicando o padrão.
    /// </summary>
    [HttpGet("metodos")]
    public Task<List<MetodoPagamentoDto>> Metodos() => _pagamentos.Metodos(User.UsuarioId());

    /// <summary>
    /// Cadastra um Pix ou cartão fictício. O primeiro cadastrado vira padrão automaticamente.
    /// </summary>
    [HttpPost("metodos")]
    public Task<MetodoPagamentoDto> Cadastrar(CreateMetodoPagamentoDto dto) =>
        _pagamentos.Cadastrar(User.UsuarioId(), dto);

    /// <summary>
    /// Simula um Pix ou cartão e credita na conta do responsável ou de um filho.
    /// </summary>
    [HttpPost("simular")]
    public async Task<IActionResult> Simular(SimularPagamentoDto dto)
    {
        await _pagamentos.Simular(User.UsuarioId(), dto);

        return NoContent();
    }
}
