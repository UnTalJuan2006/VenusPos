using System;
using System.Collections.Generic;
using System.Text;

namespace VenusPos.Application.DTOs.Historial
{
    public class ActualizarHistorialDTO
    {
        public int IdEmpleado { get; set; }
        public string Recomendaciones { get; set; } = string.Empty;
    }
}
