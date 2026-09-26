using System.ComponentModel.DataAnnotations;
using Backend.Models.Enums;

namespace Backend.DTOs.Pedidos;

// Venda no balcão, registrada pelo Admin para um aluno ou adulto
public class VendaBalcaoDto
{
    public int UsuarioId { get; set; }

    // Conta (lança no saldo) ou AVista (pagou na hora)
    public FormaPagamento FormaPagamento { get; set; }

    [MinLength(1, ErrorMessage = "Escolha pelo menos um item.")]
    public List<ItemQuantidadeDto> Itens { get; set; } = [];
}
