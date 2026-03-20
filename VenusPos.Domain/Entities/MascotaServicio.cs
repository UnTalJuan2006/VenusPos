using System;
using System.Collections.Generic;
using System.Text;

namespace VenusPos.Domain.Entities
{
    public class MascotaServicio
    {
        public int IdMascota { get; set; }

        public int IdServicio { get; set; }

        public DateTime Fecha { get; set; }

        public Mascota Mascota { get; set; }

        public Servicio Servicio { get; set; }
    }
}