namespace VenusPos.Domain.Entities
{
    public class ReservaServicio
    {
        public int IdReserva { get; set; }
        public int IdServicio { get; set; }
        public decimal PrecioUnitario { get; set; }
        public Reserva Reserva { get; set; } = null!;
        public Servicio Servicio { get; set; } = null!;
    }
}
