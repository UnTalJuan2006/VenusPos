using System;

namespace VenusPos.Domain.Entities
{
    public class ReservaMascota
    {
        public int IdReserva { get; set; }
        public int IdMascota { get; set; }

        public Reserva Reserva { get; set; }
        public Mascota Mascota { get; set; }
    }
}