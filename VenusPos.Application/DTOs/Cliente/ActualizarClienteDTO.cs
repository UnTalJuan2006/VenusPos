using System;
using System.Collections.Generic;
using System.Text;

namespace VenusPos.Application.DTOs.Cliente
{
    public class ActualizarClienteDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
      
    }
}
