using System;
using System.Collections.Generic;
using VenusPos.Domain.Enums.Mascota;

namespace VenusPos.Domain.Entities
{
    public class Mascota
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Raza { get; set; }
        public EnumTamaño Tamaño { get; set; }
        public int Edad { get; set; }
        public EnumTipoPelaje TipoPelaje { get; set; }
        public string Observaciones { get; set; }
        public DateTime FechaCreacion { get; set; }

        public DateTime FechaNacimiento { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public string Imagen { get; set; }
        public int IdCliente { get; set; }
        public Cliente Cliente { get; set; }
        public ICollection<ReservaMascota> ReservaMascotas { get; set; }
        public ICollection<MascotaServicio> MascotaServicios { get; set; }
        public ICollection<Historial> Historial{ get; set; }
    }
}