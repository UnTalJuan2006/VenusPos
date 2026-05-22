using System;
using System.Collections.Generic;
using System.Text;

namespace VenusPos.Application.DTOs.Empleado
{
    public class InactivarEmpleadoDTO
    {
        public bool Activo { get; set; }
        public DateTime InactivoDesde { get; set; }
        public DateTime InactivoHasta { get; set; }
    }
}
