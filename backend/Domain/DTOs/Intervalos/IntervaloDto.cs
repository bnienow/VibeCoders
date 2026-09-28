namespace Backend.Domain.DTOs.Intervalos;

public class IntervaloDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFim { get; set; }

    // Hora em que o intervalo para de aceitar pedido (ex.: 08:45)
    public TimeOnly Fechamento { get; set; }
}
