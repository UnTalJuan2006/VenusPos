using System;
using System.Collections.Generic;
using System.Text;

namespace VenusPos.Application.DTOs.Empleado
{
    public class EmpleadoDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime InactivoDesde { get; set; }
        public DateTime InactivoHasta { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? Imagen { get; set; }
    }
}
