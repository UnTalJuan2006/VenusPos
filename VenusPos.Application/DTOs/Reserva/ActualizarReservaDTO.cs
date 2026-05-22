namespace VenusPos.Application.DTOs.Reserva
{
    public class ActualizarReservaDTO
    {
        public int IdCliente { get; set; }
        public int IdMascota { get; set; }
        public int IdEmpleado { get; set; }
        public DateTime FechaReserva { get; set; }
        public string HoraInicio { get; set; } = string.Empty;
        public List<int> IdsServicios { get; set; } = new();
        public string? Detalles { get; set; }
        /// <summary>
        /// Duración personalizada en minutos. Si se proporciona, se usa esta duración
        /// en lugar de calcularla automáticamente según el tamaño y tipo de pelaje.
        /// </summary>
        public int? DuracionPersonalizadaMinutos { get; set; }
    }
}
