using Backend.Domain.Models.Enums;

namespace Backend.Domain.Models;

public class Fechamento
{
    public int Id { get; set; }
    public int AdultoId { get; set; }

    // Primeiro dia do mês fechado
    public DateOnly MesReferencia { get; set; }

    public decimal ValorTotal { get; set; }
    public StatusFechamento Status { get; set; }
    public DateTime? GeradoEm { get; set; }
    public DateTime? PagoEm { get; set; }

    public Adulto Adulto { get; set; } = null!;
}
