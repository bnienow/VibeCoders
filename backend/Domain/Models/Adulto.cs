namespace Backend.Domain.Models;

public class Adulto
{
    public int UsuarioId { get; set; }
    public string Cpf { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;

    public int? MetodoPagamentoPadraoId { get; set; }

    public Usuario Usuario { get; set; } = null!;
    public MetodoPagamento? MetodoPagamentoPadrao { get; set; }
    public ICollection<Aluno> Filhos { get; set; } = new List<Aluno>();
    public ICollection<MetodoPagamento> MetodosPagamento { get; set; } = new List<MetodoPagamento>();
    public ICollection<Fechamento> Fechamentos { get; set; } = new List<Fechamento>();
}
