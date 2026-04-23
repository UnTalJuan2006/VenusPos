using System;
using System.Collections.Generic;
using System.Text;

namespace VenusPos.Application.DTOs.Historial
{
    public class CrearHistorialDTO
    {
        public int IdEmpleado { get; set; }
        public int IdMascota { get; set; }
        public string Recomendaciones { get; set; } = string.Empty;
        public int? IdReserva { get; set; }
    }
}
