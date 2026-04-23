namespace VenusPos.Application.DTOs.Reserva
{
    public class HorarioDisponibleDTO
    {
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
        public List<EmpleadoDisponibleDTO> EmpleadosDisponibles { get; set; } = new();
        public bool EstaDisponible { get; set; } = true;
        public string MotivoNoDisponible { get; set; } = string.Empty;
    }
}
