namespace VenusPos.Domain.Entities
{
    public class EmpleadoServicio
    {
        public int IdEmpleado { get; set; }

        public int IdServicio { get; set; }

        public Empleado Empleado { get; set; }

        public Servicio Servicio { get; set; }
    }
}