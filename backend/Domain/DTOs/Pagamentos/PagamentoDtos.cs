using System.ComponentModel.DataAnnotations;
using Backend.Domain.Models.Enums;

namespace Backend.Domain.DTOs.Pagamentos;

public class MetodoPagamentoDto
{
    public int Id { get; set; }
    public TipoMetodoPagamento Tipo { get; set; }
    public string Apelido { get; set; } = string.Empty;
    public string UltimosDigitos { get; set; } = string.Empty;
    public bool Padrao { get; set; }
}

// Cadastro de Pix ou cartão fictício
public class CreateMetodoPagamentoDto
{
    public TipoMetodoPagamento Tipo { get; set; }

    [Required(ErrorMessage = "Informe um apelido para o método.")]
    public string Apelido { get; set; } = string.Empty;

    public string UltimosDigitos { get; set; } = string.Empty;

    // Marca como o método padrão do responsável
    public bool Padrao { get; set; }
}

// Crédito simulado na conta do responsável ou de um filho
public class SimularPagamentoDto
{
    // Conta que recebe o crédito: o próprio responsável ou um filho
    public int UsuarioId { get; set; }

    [Range(1, 1000, ErrorMessage = "O crédito precisa ser entre R$ 1,00 e R$ 1.000,00.")]
    public decimal Valor { get; set; }

    public int? MetodoPagamentoId { get; set; }
}
