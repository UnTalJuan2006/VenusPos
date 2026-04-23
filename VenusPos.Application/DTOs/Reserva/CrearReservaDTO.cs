namespace VenusPos.Application.DTOs.Reserva
{
    public class CrearReservaDTO
    {
        public int IdCliente { get; set; }
        public int IdMascota { get; set; }
        public int IdEmpleado { get; set; }
        public DateTime FechaReserva { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public List<int> IdsServicios { get; set; } = new();
        public string Detalles { get; set; } = string.Empty;
    }
}
