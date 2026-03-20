using System;
using System.Collections.Generic;
using System.Text;

namespace VenusPos.Application.DTOs.Mascota
{
    public class MascotaDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Raza { get; set; } = string.Empty;
        public string Tamaño { get; set; } = string.Empty;
        public int Edad { get; set; }
        public string TipoPelaje { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? Imagen { get; set; }
        public int IdCliente { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
    }
}
