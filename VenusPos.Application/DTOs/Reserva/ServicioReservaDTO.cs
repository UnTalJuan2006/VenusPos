namespace VenusPos.Application.DTOs.Reserva
{
    public class ServicioReservaDTO
    {
        public int IdServicio { get; set; }
        public string NombreServicio { get; set; } = string.Empty;
        public decimal PrecioUnitario { get; set; }
    }
}
