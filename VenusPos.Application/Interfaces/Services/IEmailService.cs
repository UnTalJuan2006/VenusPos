using VenusPos.Domain.Entities;

namespace VenusPos.Application.Interfaces.Services
{
    public interface IEmailService
    {
        Task EnviarConfirmacionReservaAsync(Reserva reserva);
    }
}
