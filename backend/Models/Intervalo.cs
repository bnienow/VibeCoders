namespace Backend.Models;

public class Intervalo
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFim { get; set; }

    // Pedidos antecipados fecham este tanto de minutos antes de HoraInicio
    public int MinutosAntecedencia { get; set; }
}
