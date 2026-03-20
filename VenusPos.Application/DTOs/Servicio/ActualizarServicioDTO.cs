using System;
using System.Collections.Generic;
using System.Text;

namespace VenusPos.Application.DTOs.Servicio
{
    public class ActualizarServicioDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public bool Activo { get; set; }
        
    }
}
