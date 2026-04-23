using System;
using System.Collections.Generic;
using System.Text;

namespace VenusPos.Application.DTOs.Caja
{
    public class CerrarCajaDTO
    {
        public decimal MontoCierre { get; set; }
        public string? Observaciones { get; set; }
    }
}
