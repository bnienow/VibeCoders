namespace Backend.Domain.Models;

public class Conta
{
    public int Id { get; set; }

    // Uma conta por usuário comprador (aluno ou adulto)
    public int UsuarioId { get; set; }

    // Positivo = crédito; negativo = fiado (mínimo -250 só para aluno; adulto sem teto)
    public decimal Saldo { get; set; }

    public bool Ativa { get; set; } = true;
    public DateTime AtualizadaEm { get; set; }

    public Usuario Usuario { get; set; } = null!;
    public ICollection<Movimento> Movimentos { get; set; } = new List<Movimento>();
}
