using Backend.Domain.DTOs.Intervalos;

namespace Backend.Domain.Interfaces.Services;

public interface IIntervaloService
{
    // Intervalos do dia com horários e a hora em que param de aceitar pedido
    Task<List<IntervaloDto>> Listar();
}
