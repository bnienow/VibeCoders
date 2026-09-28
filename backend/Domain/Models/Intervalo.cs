namespace Backend.Domain.Models;

public class Intervalo
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFim { get; set; }

    // Pedidos antecipados fecham este tanto de minutos antes de HoraInicio
    public int MinutosAntecedencia { get; set; }

    // Momento em que o intervalo para de aceitar pedido: dia + hora de início − antecedência (ex.: 08:45)
    public DateTime FechamentoEm(DateOnly data) =>
        data.ToDateTime(HoraInicio).AddMinutes(-MinutosAntecedencia);
}
