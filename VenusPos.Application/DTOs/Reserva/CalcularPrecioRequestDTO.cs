namespace VenusPos.Application.DTOs.Reserva
{
    public class CalcularPrecioRequestDTO
    {
        public int IdMascota { get; set; }
        public List<int> IdsServicios { get; set; } = new();
    }
}
