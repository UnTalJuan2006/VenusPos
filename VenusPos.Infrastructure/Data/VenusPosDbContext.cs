using Microsoft.EntityFrameworkCore;
using VenusPos.Domain.Entities;

namespace VenusPos.Infrastructure.Data
{
    public class VenusPosDbContext : DbContext
    {
        public VenusPosDbContext(DbContextOptions<VenusPosDbContext> options)
            : base(options)
        {
        }

        // TABLAS PRINCIPALES
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Mascota> Mascotas { get; set; }
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Servicio> Servicios { get; set; }
        public DbSet<Reserva> Reservas { get; set; }

        // TABLAS RELACIONALES
        public DbSet<ReservaMascota> ReservaMascotas { get; set; }
        public DbSet<MascotaServicio> MascotaServicios { get; set; }
        public DbSet<EmpleadoServicio> EmpleadoServicios { get; set; }

        // CAJA Y VENTAS
        public DbSet<Caja> Caja { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<VentaDetalle> VentaDetalles { get; set; }
        public DbSet<MovimientoCaja> MovimientosCaja { get; set; }

        // HISTORIAL
        public DbSet<Historial> Historiales { get; set; }

        // CONFIGURACION
        public DbSet<ConfiguracionPrecio> ConfiguracionesPrecios { get; set; }
        public DbSet<ReservaServicio> ReservaServicios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ================================
            // PRECISION DECIMAL
            // ================================
            modelBuilder.Entity<Servicio>()
                .Property(s => s.Precio)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Caja>().Property(c => c.MontoApertura).HasPrecision(10, 2);
            modelBuilder.Entity<Caja>().Property(c => c.MontoCierre).HasPrecision(10, 2);
            modelBuilder.Entity<Caja>().Property(c => c.Faltante).HasPrecision(10, 2);
            modelBuilder.Entity<Caja>().Property(c => c.Sobrante).HasPrecision(10, 2);
            modelBuilder.Entity<Caja>().Property(c => c.TotalVentas).HasPrecision(10, 2);
            modelBuilder.Entity<Caja>().Property(c => c.TotalEfectivo).HasPrecision(10, 2);
            modelBuilder.Entity<Caja>().Property(c => c.TotalTarjeta).HasPrecision(10, 2);
            modelBuilder.Entity<Caja>().Property(c => c.TotalTransferencia).HasPrecision(10, 2);

            modelBuilder.Entity<Venta>().Property(v => v.Subtotal).HasPrecision(10, 2);
            modelBuilder.Entity<Venta>().Property(v => v.Descuento).HasPrecision(10, 2);
            modelBuilder.Entity<Venta>().Property(v => v.Total).HasPrecision(10, 2);

            modelBuilder.Entity<VentaDetalle>().Property(vd => vd.PrecioUnitario).HasPrecision(10, 2);
            modelBuilder.Entity<VentaDetalle>().Property(vd => vd.Subtotal).HasPrecision(10, 2);

            modelBuilder.Entity<Reserva>().Property(r => r.PrecioTotal).HasPrecision(10, 2);

            modelBuilder.Entity<ReservaServicio>().Property(rs => rs.PrecioUnitario).HasPrecision(10, 2);

            modelBuilder.Entity<ConfiguracionPrecio>().Property(cp => cp.Valor).HasPrecision(5, 4);

            modelBuilder.Entity<MovimientoCaja>().Property(mc => mc.Monto).HasPrecision(10, 2);

            // ================================
            // ENUMS COMO STRING - MASCOTA
            // ================================
            modelBuilder.Entity<Mascota>()
                .Property(m => m.Tamaño)
                .HasConversion<string>();

            modelBuilder.Entity<Mascota>()
                .Property(m => m.TipoPelaje)
                .HasConversion<string>();

            // ================================
            // ENUMS COMO STRING - RESERVA
            // ================================
            modelBuilder.Entity<Reserva>()
                .Property(r => r.Estado)
                .HasConversion<string>();

            // ================================
            // CLAVES COMPUESTAS
            // ================================
            modelBuilder.Entity<ReservaMascota>()
                .HasKey(rm => new { rm.IdReserva, rm.IdMascota });

            modelBuilder.Entity<MascotaServicio>()
                .HasKey(ms => new { ms.IdMascota, ms.IdServicio });

            modelBuilder.Entity<EmpleadoServicio>()
                .HasKey(es => new { es.IdEmpleado, es.IdServicio });

            modelBuilder.Entity<ReservaServicio>()
                .HasKey(rs => new { rs.IdReserva, rs.IdServicio });

            // ================================
            // RELACIONES - MASCOTA
            // ================================
            modelBuilder.Entity<Mascota>()
                .HasOne(m => m.Cliente)
                .WithMany(c => c.Mascota)
                .HasForeignKey(m => m.IdCliente)
                .OnDelete(DeleteBehavior.Restrict);

            // ================================
            // RELACIONES - RESERVA
            // ================================
            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.Cliente)
                .WithMany(c => c.Reserva)
                .HasForeignKey(r => r.IdCliente)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.Empleado)
                .WithMany(e => e.Reserva)
                .HasForeignKey(r => r.IdEmpleado)
                .OnDelete(DeleteBehavior.Restrict);

            // ================================
            // RELACIONES - RESERVA MASCOTA
            // ================================
            modelBuilder.Entity<ReservaMascota>()
                .HasOne(rm => rm.Reserva)
                .WithMany(r => r.ReservaMascotas)
                .HasForeignKey(rm => rm.IdReserva)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReservaMascota>()
                .HasOne(rm => rm.Mascota)
                .WithMany(m => m.ReservaMascotas)
                .HasForeignKey(rm => rm.IdMascota)
                .OnDelete(DeleteBehavior.Restrict);

            // ================================
            // RELACIONES - MASCOTA SERVICIO
            // ================================
            modelBuilder.Entity<MascotaServicio>()
                .HasOne(ms => ms.Mascota)
                .WithMany(m => m.MascotaServicios)
                .HasForeignKey(ms => ms.IdMascota)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MascotaServicio>()
                .HasOne(ms => ms.Servicio)
                .WithMany(s => s.MascotaServicios)
                .HasForeignKey(ms => ms.IdServicio)
                .OnDelete(DeleteBehavior.Restrict);

            // ================================
            // RELACIONES - EMPLEADO SERVICIO
            // ================================
            modelBuilder.Entity<EmpleadoServicio>()
                .HasOne(es => es.Empleado)
                .WithMany(e => e.EmpleadoServicios)
                .HasForeignKey(es => es.IdEmpleado)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmpleadoServicio>()
                .HasOne(es => es.Servicio)
                .WithMany(s => s.EmpleadoServicios)
                .HasForeignKey(es => es.IdServicio)
                .OnDelete(DeleteBehavior.Restrict);

            // ================================
            // RELACIONES - RESERVA SERVICIO
            // ================================
            modelBuilder.Entity<ReservaServicio>()
                .HasOne(rs => rs.Reserva)
                .WithMany(r => r.ReservaServicios)
                .HasForeignKey(rs => rs.IdReserva)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReservaServicio>()
                .HasOne(rs => rs.Servicio)
                .WithMany()
                .HasForeignKey(rs => rs.IdServicio)
                .OnDelete(DeleteBehavior.Restrict);

            // ================================
            // RELACIONES - CAJA
            // ================================
            modelBuilder.Entity<Caja>()
                .HasOne(c => c.Empleado)
                .WithMany(e => e.Caja)
                .HasForeignKey(c => c.IdEmpleado)
                .OnDelete(DeleteBehavior.Restrict);

            // ================================
            // RELACIONES - MOVIMIENTO CAJA
            // ================================
            modelBuilder.Entity<MovimientoCaja>()
                .HasOne(mc => mc.Caja)
                .WithMany(c => c.MovimientosCaja)
                .HasForeignKey(mc => mc.IdCaja)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimientoCaja>()
                .HasOne(mc => mc.Empleado)
                .WithMany()
                .HasForeignKey(mc => mc.IdEmpleado)
                .OnDelete(DeleteBehavior.Restrict);

            // ================================
            // RELACIONES - VENTA
            // ================================
            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Caja)
                .WithMany(c => c.Ventas)
                .HasForeignKey(v => v.IdCaja)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Reserva)
                .WithMany(r => r.Ventas)
                .HasForeignKey(v => v.IdReserva)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Cliente)
                .WithMany(c => c.Ventas)
                .HasForeignKey(v => v.IdCliente)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Empleado)
                .WithMany(e => e.Ventas)
                .HasForeignKey(v => v.IdEmpleado)
                .OnDelete(DeleteBehavior.Restrict);

            // ================================
            // RELACIONES - VENTA DETALLE
            // ================================
            modelBuilder.Entity<VentaDetalle>()
                .HasOne(vd => vd.Venta)
                .WithMany(v => v.Detalles)
                .HasForeignKey(vd => vd.IdVenta)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VentaDetalle>()
                .HasOne(vd => vd.Servicio)
                .WithMany(s => s.VentaDetalles)
                .HasForeignKey(vd => vd.IdServicio)
                .OnDelete(DeleteBehavior.Restrict);

            // ================================
            // RELACIONES - HISTORIAL
            // ================================
            modelBuilder.Entity<Historial>()
                .HasOne(h => h.Empleado)
                .WithMany(e => e.Historial)
                .HasForeignKey(h => h.IdEmpleado)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Historial>()
                .HasOne(h => h.Mascota)
                .WithMany(m => m.Historial)
                .HasForeignKey(h => h.IdMascota)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Historial>()
                .HasOne(h => h.Reserva)
                .WithMany(r => r.Historiales)
                .HasForeignKey(h => h.IdReserva)
                .OnDelete(DeleteBehavior.SetNull);

            // ================================
            // INDICES DE OPTIMIZACION
            // ================================
            modelBuilder.Entity<Reserva>()
                .HasIndex(r => r.CodigoReserva)
                .IsUnique()
                .HasFilter("[CodigoReserva] IS NOT NULL");

            modelBuilder.Entity<Reserva>()
                .HasIndex(r => new { r.FechaReserva, r.IdEmpleado });

            modelBuilder.Entity<ConfiguracionPrecio>()
                .HasIndex(cp => cp.Clave)
                .IsUnique();
        }
    }
}