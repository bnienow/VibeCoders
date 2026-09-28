using Backend.Domain.DTOs.Pedidos;

namespace Backend.Domain.DTOs.Painel;

// Tela principal da cantina: pedidos do próximo intervalo
public class PainelDto
{
    // Nulo quando não há mais intervalo hoje
    public string? Intervalo { get; set; }
    public TimeOnly? HoraInicio { get; set; }
    public TimeOnly? HoraFim { get; set; }
    public int Pendentes { get; set; }
    public int Entregues { get; set; }
    public List<PedidoDto> Pedidos { get; set; } = [];
}

// Uma linha da coluna "Preparo": quanto a cozinha precisa montar de cada item
public class PreparoItemDto
{
    public string Nome { get; set; } = string.Empty;
    public int Quantidade { get; set; }
}
