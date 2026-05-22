using System;
using System.Collections.Generic;
using System.Text;

namespace VenusPos.Application.DTOs.Empleado
{
    public class CrearEmpleadoDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public string? Imagen { get; set; }
        public bool Activo { get; set; } = true;
    }
}