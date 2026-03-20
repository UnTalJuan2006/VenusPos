using System;
using System.Collections.Generic;
using System.Text;

namespace VenusPos.Application.DTOs.Empleado
{
    public class ActualizarEmpleadoDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public string? Imagen { get; set; }
    }
}