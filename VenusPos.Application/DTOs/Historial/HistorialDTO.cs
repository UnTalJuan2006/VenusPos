using System;
using System.Collections.Generic;
using System.Text;
using VenusPos.Application.DTOs.Mascota;
using VenusPos.Application.DTOs.Empleado; 
using VenusPos.Application.DTOs.Reserva;

namespace VenusPos.Application.DTOs.Historial
{
    public class HistorialDTO
    {
        public int Id { get; set; }
        public MascotaDTO Mascota { get; set; } = null!;
        public int IdEmpleado { get; set; }
        public string NombreEmpleado { get; set; } = string.Empty;
        public string CargoEmpleado { get; set; } = string.Empty;
        public string ImagenEmpleado { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int? IdReserva { get; set; }
        public DateTime? FechaReserva { get; set; }
        public string? CodigoReserva { get; set; }
        public string Recomendaciones { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaAtencion { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }
}
