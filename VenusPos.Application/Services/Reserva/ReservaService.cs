
using VenusPos.Application.DTOs.Reserva;
using VenusPos.Application.Interfaces.Repositories;
using VenusPos.Application.Interfaces.Services;
using VenusPos.Domain.Enums.Mascota;
using VenusPos.Domain.Enums.Reserva;
using ReservaEntity = VenusPos.Domain.Entities.Reserva;

namespace VenusPos.Application.Services.Reserva
{
    public class ReservaService : IReservaService
    {
        private readonly IReservaRepository _reservaRepo;
        private readonly IMascotaRepository _mascotaRepo;
        private readonly IServicioRepository _servicioRepo;
        private readonly IEmpleadoRepository _empleadoRepo;
        private readonly IConfiguracionPrecioRepository _configRepo;
        private readonly IEmailService _emailService;
        private readonly IVentaService _ventaService;
        private readonly INotificacionService _notificacionService;

        public ReservaService(
            IReservaRepository reservaRepo,
            IMascotaRepository mascotaRepo,
            IServicioRepository servicioRepo,
            IEmpleadoRepository empleadoRepo,
            IConfiguracionPrecioRepository configRepo,
            IEmailService emailService,
            IVentaService ventaService,
            INotificacionService notificacionService)
        {
            _reservaRepo = reservaRepo;
            _mascotaRepo = mascotaRepo;
            _servicioRepo = servicioRepo;
            _empleadoRepo = empleadoRepo;
            _configRepo = configRepo;
            _emailService = emailService;
            _ventaService = ventaService;
            _notificacionService = notificacionService;
        }

        public async Task<IEnumerable<ReservaDTO>> ObtenerTodosAsync()
        {
            var reservas = await _reservaRepo.ObtenerTodosAsync();
            return reservas.Select(MapToDTO);
        }

        public async Task<ReservaDTO?> ObtenerPorIdAsync(int id)
        {
            var reserva = await _reservaRepo.ObtenerPorIdAsync(id);
            return reserva is null ? null : MapToDTO(reserva);
        }

        public async Task<ReservaDTO?> ObtenerPorCodigoAsync(string codigo)
        {
            var reserva = await _reservaRepo.ObtenerPorCodigoAsync(codigo);
            return reserva is null ? null : MapToDTO(reserva);
        }

        public async Task<IEnumerable<ReservaDTO>> ObtenerPorClienteAsync(int idCliente)
        {
            var reservas = await _reservaRepo.ObtenerPorClienteAsync(idCliente);
            return reservas.Select(MapToDTO);
        }

        public async Task<IEnumerable<ReservaDTO>> ObtenerPorMascotaAsync(int idMascota)
        {
            var reservas = await _reservaRepo.ObtenerPorMascotaAsync(idMascota);
            return reservas.Select(MapToDTO);
        }

        public async Task<ReservaDTO> CrearReservaAsync(CrearReservaDTO dto)
        {
            // 1. Validar que la mascota existe y pertenece al cliente
            var mascota = await _mascotaRepo.ObtenerPorIdAsync(dto.IdMascota)
                ?? throw new KeyNotFoundException("Mascota no encontrada");

            if (mascota.IdCliente != dto.IdCliente)
                throw new InvalidOperationException("La mascota no pertenece al cliente");

            // 2. Validar que el empleado existe
            var empleado = await _empleadoRepo.ObtenerPorIdAsync(dto.IdEmpleado);
            if (empleado is null)
                throw new KeyNotFoundException("Empleado no encontrado");

            // 3. Validar que la fecha no sea pasada
            if (dto.FechaReserva.Date < DateTime.Today)
                throw new InvalidOperationException("La fecha de reserva no puede ser pasada");

            // 4. Calcular duración según tamaño y tipo de pelaje
            var duracionMinutos = CalcularDuracionMinutos(mascota.Tamaño, mascota.TipoPelaje);
            var horaFin = dto.HoraInicio.AddMinutes(duracionMinutos);

            // 5. Validar disponibilidad
            if (!await ValidarDisponibilidadAsync(dto.IdEmpleado, dto.FechaReserva, dto.HoraInicio, horaFin))
                throw new InvalidOperationException("El horario seleccionado no está disponible");

            // 6. Calcular precio
            var calculo = await CalcularPrecioReservaAsync(dto.IdMascota, dto.IdsServicios);

            // 7. Obtener servicios para guardar precios unitarios
            var servicios = await _servicioRepo.ObtenerPorIdsAsync(dto.IdsServicios);

            // 8. Generar código de reserva
            var codigoReserva = await GenerarCodigoReservaAsync(dto.FechaReserva);

            // 9. Crear entidad Reserva con relaciones (directamente como Confirmada)
            var reserva = new ReservaEntity
            {
                IdCliente = dto.IdCliente,
                IdEmpleado = dto.IdEmpleado,
                FechaReserva = dto.FechaReserva,
                HoraInicio = dto.HoraInicio,
                HoraFin = horaFin,
                Estado = EnumEstado.Confirmada,
                CodigoReserva = codigoReserva,
                Detalles = dto.Detalles,
                PrecioTotal = calculo.PrecioFinal,
                DuracionMinutos = duracionMinutos,
                FechaCreacion = DateTime.UtcNow,
                ReservaMascotas = new List<Domain.Entities.ReservaMascota>
                {
                    new Domain.Entities.ReservaMascota
                    {
                        IdMascota = dto.IdMascota
                    }
                },
                ReservaServicios = servicios.Select(s => new Domain.Entities.ReservaServicio
                {
                    IdServicio = s.Id,
                    PrecioUnitario = s.Precio
                }).ToList()
            };

            // 9. Guardar reserva con todas las relaciones
            var reservaCreada = await _reservaRepo.CrearAsync(reserva);

            // 10. Generar notificación de nueva reserva
            await _notificacionService.CrearNotificacionReservaAsync(
                idReserva: reservaCreada.Id,
                titulo: "Nueva Reserva",
                mensaje: $"Nueva reserva creada: {reservaCreada.CodigoReserva}",
                tipo: "NuevaReserva"
            );

            // 11. Retornar DTO
            return MapToDTO(reservaCreada);
        }

        public async Task<ReservaDTO> ConfirmarReservaAsync(int id)
        {
            var reserva = await _reservaRepo.ObtenerPorIdAsync(id)
                ?? throw new KeyNotFoundException("Reserva no encontrada");

            if (reserva.Estado != EnumEstado.Pendiente)
                throw new InvalidOperationException("Solo se pueden confirmar reservas en estado Pendiente");

            // 1. Generar código de reserva
            reserva.CodigoReserva = await GenerarCodigoReservaAsync(reserva.FechaReserva);
            reserva.Estado = EnumEstado.Confirmada;
            reserva.FechaActualizacion = DateTime.UtcNow;

            // 2. Actualizar en BD
            var actualizada = await _reservaRepo.ActualizarAsync(reserva);

            // 3. Enviar email de confirmación
            await _emailService.EnviarConfirmacionReservaAsync(actualizada);

            return MapToDTO(actualizada);
        }

        public async Task<ReservaDTO> ActualizarEstadoAsync(int id, ActualizarEstadoReservaDTO dto)
        {
            var reserva = await _reservaRepo.ObtenerPorIdAsync(id)
                ?? throw new KeyNotFoundException("Reserva no encontrada");

            // Validar transición de estado válida
            if (!Enum.TryParse<EnumEstado>(dto.Estado, out var nuevoEstado))
                throw new ArgumentException($"Estado '{dto.Estado}' no es válido");

            reserva.Estado = nuevoEstado;
            reserva.FechaActualizacion = DateTime.UtcNow;

            var actualizada = await _reservaRepo.ActualizarAsync(reserva);
            return MapToDTO(actualizada);
        }

        public async Task<ReservaDTO> ActualizarReservaAsync(int id, ActualizarReservaDTO dto)
        {
            // 1. Validar que la reserva existe
            var reserva = await _reservaRepo.ObtenerPorIdAsync(id)
                ?? throw new KeyNotFoundException("Reserva no encontrada");

            // 2. Validar que la mascota existe y pertenece al cliente
            var mascota = await _mascotaRepo.ObtenerPorIdAsync(dto.IdMascota)
                ?? throw new KeyNotFoundException("Mascota no encontrada");

            if (mascota.IdCliente != dto.IdCliente)
                throw new InvalidOperationException("La mascota no pertenece al cliente");

            // 3. Validar que el empleado existe
            var empleado = await _empleadoRepo.ObtenerPorIdAsync(dto.IdEmpleado);
            if (empleado is null)
                throw new KeyNotFoundException("Empleado no encontrado");

            // 4. Calcular duración según tamaño y tipo de pelaje, o usar la personalizada si se proporciona
            var duracionMinutos = dto.DuracionPersonalizadaMinutos ?? CalcularDuracionMinutos(mascota.Tamaño, mascota.TipoPelaje);
            var horaInicio = TimeOnly.Parse(dto.HoraInicio);
            var horaFin = horaInicio.AddMinutes(duracionMinutos);

            // 5. Validar disponibilidad (solo si cambió empleado, fecha u hora)
            if (reserva.IdEmpleado != dto.IdEmpleado ||
                reserva.FechaReserva.Date != dto.FechaReserva.Date ||
                reserva.HoraInicio != horaInicio)
            {
                if (!await ValidarDisponibilidadAsync(dto.IdEmpleado, dto.FechaReserva, horaInicio, horaFin))
                    throw new InvalidOperationException("El horario seleccionado no está disponible");
            }

            // 6. Calcular precio
            var calculo = await CalcularPrecioReservaAsync(dto.IdMascota, dto.IdsServicios);

            // 7. Obtener servicios para guardar precios unitarios
            var servicios = await _servicioRepo.ObtenerPorIdsAsync(dto.IdsServicios);

            // 8. Actualizar entidad Reserva
            reserva.IdCliente = dto.IdCliente;
            reserva.IdEmpleado = dto.IdEmpleado;
            reserva.FechaReserva = dto.FechaReserva;
            reserva.HoraInicio = horaInicio;
            reserva.HoraFin = horaFin;
            reserva.Detalles = dto.Detalles;
            reserva.PrecioTotal = calculo.PrecioFinal;
            reserva.DuracionMinutos = duracionMinutos;
            reserva.FechaActualizacion = DateTime.UtcNow;

            // 9. Actualizar mascotas (limpiar y agregar nueva)
            reserva.ReservaMascotas.Clear();
            reserva.ReservaMascotas.Add(new Domain.Entities.ReservaMascota
            {
                IdReserva = reserva.Id,
                IdMascota = dto.IdMascota
            });

            // 10. Actualizar servicios (limpiar y agregar nuevos)
            reserva.ReservaServicios.Clear();
            foreach (var s in servicios)
            {
                reserva.ReservaServicios.Add(new Domain.Entities.ReservaServicio
                {
                    IdReserva = reserva.Id,
                    IdServicio = s.Id,
                    PrecioUnitario = s.Precio
                });
            }

            // 11. Guardar cambios
            var actualizada = await _reservaRepo.ActualizarAsync(reserva);

            // 12. Retornar DTO
            return MapToDTO(actualizada);
        }

        public async Task<bool> CancelarReservaAsync(int id)
        {
            var reserva = await _reservaRepo.ObtenerPorIdAsync(id);
            if (reserva is null)
                return false;

            if (reserva.Estado == EnumEstado.Completada)
                throw new InvalidOperationException("No se puede cancelar una reserva completada");

            // 1. Anular venta asociada si existe
            await _ventaService.AnularVentaPorReservaAsync(id);

            // 2. Actualizar estado de la reserva
            reserva.Estado = EnumEstado.Cancelada;
            reserva.FechaActualizacion = DateTime.UtcNow;

            await _reservaRepo.ActualizarAsync(reserva);

            // 3. Generar notificación
            await _notificacionService.CrearNotificacionReservaAsync(
                idReserva: id,
                titulo: "Reserva Cancelada",
                mensaje: $"La reserva {reserva.CodigoReserva} ha sido cancelada",
                tipo: "ReservaCancelada"
            );

            return true;
        }

        public async Task<CalculoPrecioDTO> CalcularPrecioReservaAsync(int idMascota, List<int> idsServicios)
        {
            // 1. Obtener mascota con sus características
            var mascota = await _mascotaRepo.ObtenerPorIdAsync(idMascota)
                ?? throw new KeyNotFoundException("Mascota no encontrada");

            // 2. Obtener servicios solicitados
            var servicios = await _servicioRepo.ObtenerPorIdsAsync(idsServicios);
            if (!servicios.Any())
                throw new InvalidOperationException("No se encontraron servicios activos");

            // 3. Separar servicio base (Baño Completo) de servicios adicionales
            var servicioBase = servicios.FirstOrDefault(s => s.Nombre == "Baño Completo");
            var serviciosAdicionales = servicios.Where(s => s.Nombre != "Baño Completo").ToList();

            decimal precioServicioBase = servicioBase?.Precio ?? 0m;
            decimal precioServiciosAdicionales = serviciosAdicionales.Sum(s => s.Precio);

            // 4. Obtener multiplicadores según nueva lógica
            decimal multTamaño = await ObtenerMultiplicadorTamañoAsync(mascota.Tamaño, mascota.TipoPelaje);
            decimal multPelaje = await ObtenerMultiplicadorPelajeAsync(mascota.Tamaño, mascota.TipoPelaje);

            // 6. Calcular recargo SOLO sobre el servicio base
            // El recargo es la suma de ambos porcentajes aplicada sobre el servicio base
            decimal recargoServicioBase = precioServicioBase * (multTamaño + multPelaje);

            // 7. Calcular precio final
            // Precio final = (Servicio Base + Recargo) + Servicios Adicionales
            decimal precioFinal = (precioServicioBase + recargoServicioBase) + precioServiciosAdicionales;

            // 8. Calcular duración según tamaño y tipo de pelaje
            int duracionMinutos = CalcularDuracionMinutos(mascota.Tamaño, mascota.TipoPelaje);

            // 9. Generar detalle de cálculo mejorado
            decimal precioBase = precioServicioBase + precioServiciosAdicionales;
            string detalle = $"Servicio Base: ${precioServicioBase:N0} | Recargo Tamaño+Pelaje: ${recargoServicioBase:N0} | Servicios Adicionales: ${precioServiciosAdicionales:N0}";

            return new CalculoPrecioDTO
            {
                IdMascota = idMascota,
                IdsServicios = idsServicios,
                PrecioBase = precioBase,
                MultiplicadorTamaño = multTamaño,
                MultiplicadorPelaje = multPelaje,
                PrecioFinal = precioFinal,
                DuracionMinutos = duracionMinutos,
                DetalleCalculo = detalle
            };
        }

        public async Task<List<HorarioDisponibleDTO>> ObtenerHorariosDisponiblesAsync(
            DateTime fecha, int duracionMinutos, int? idEmpleado = null, EnumTamaño? tamañoMascota = null)
        {
            var horarios = new List<HorarioDisponibleDTO>();

            // No permitir domingos
            if (fecha.DayOfWeek == DayOfWeek.Sunday)
                return horarios;

            // Horarios según día de la semana
            TimeOnly horaApertura;
            TimeOnly horaCierre;

            if (fecha.DayOfWeek == DayOfWeek.Saturday)
            {
                // Sábados: 10:00 AM - 4:00 PM
                horaApertura = new TimeOnly(10, 0);
                horaCierre = new TimeOnly(16, 0);
            }
            else
            {
                // Lunes a Viernes: 9:00 AM - 5:30 PM
                horaApertura = new TimeOnly(9, 0);
                horaCierre = new TimeOnly(17, 30);
            }

            // Filtrar horarios pasados si es el día actual
            var ahora = DateTime.Now;
            var horaActual = new TimeOnly(ahora.Hour, ahora.Minute);
            if (fecha.Date == ahora.Date)
            {
                // Ajustar hora de apertura al siguiente bloque de 30 minutos después de la hora actual
                var minutosDesdeMedianoche = horaActual.Hour * 60 + horaActual.Minute;
                var siguienteBloque = (int)Math.Ceiling(minutosDesdeMedianoche / 30.0) * 30;
                var horaCalculada = new TimeOnly(siguienteBloque / 60, siguienteBloque % 60);

                // Usar la hora más tardía entre la calculada y la de apertura
                if (horaCalculada > horaApertura)
                    horaApertura = horaCalculada;
            }

            // Generar bloques de 30 minutos con la duración solicitada (según tamaño del perro)
            for (var hora = horaApertura; hora < horaCierre; hora = hora.AddMinutes(30))
            {
                var horaFin = hora.AddMinutes(duracionMinutos);

                // Verificar que el bloque completo cabe antes del cierre
                if (horaFin > horaCierre)
                    break;

                // Obtener empleados disponibles en este horario
                var empleadosDisp = await ObtenerEmpleadosDisponiblesAsync(fecha, hora, horaFin, idEmpleado, tamañoMascota);

                // El horario está disponible si hay al menos un empleado disponible
                // No importa si otros empleados están ocupados, solo importa que haya al menos uno libre
                var horarioDTO = new HorarioDisponibleDTO
                {
                    HoraInicio = hora,
                    HoraFin = horaFin,
                    EmpleadosDisponibles = empleadosDisp,
                    EstaDisponible = empleadosDisp.Any(),
                    MotivoNoDisponible = !empleadosDisp.Any() ? "Todos los empleados ocupados" : string.Empty
                };

                horarios.Add(horarioDTO);
            }

            return horarios;
        }

        // ==============================
        // MÉTODOS PRIVADOS AUXILIARES
        // ==============================

        private int CalcularDuracionMinutos(EnumTamaño tamaño, EnumTipoPelaje tipoPelaje)
        {
            // Lógica de duración ACTUALIZADA:
            // Perro pequeño:
            //   - Pelo corto: 60 min (1 hora)
            //   - Pelo semi-largo: 90 min (1.5 horas)
            //   - Pelo largo: 120 min (2 horas)
            //   - Doble capa: 120 min (2 horas)
            // Perro mediano:
            //   - Pelo corto: 120 min (2 horas)
            //   - Pelo semi-largo: 150 min (2.5 horas)
            //   - Pelo largo: 150 min (2.5 horas)
            //   - Doble capa: 180 min (3 horas)
            // Perro grande:
            //   - Pelo corto: 180 min (3 horas)
            //   - Pelo semi-largo: 210 min (3.5 horas)
            //   - Pelo largo: 210 min (3.5 horas)
            //   - Doble capa: 240 min (4 horas)

            return (tamaño, tipoPelaje) switch
            {
                // Perros pequeños
                (EnumTamaño.Pequeno, EnumTipoPelaje.Corto) => 60,
                (EnumTamaño.Pequeno, EnumTipoPelaje.SemiLargo) => 90,
                (EnumTamaño.Pequeno, EnumTipoPelaje.Largo) => 120,
                (EnumTamaño.Pequeno, EnumTipoPelaje.DobleCapa) => 160,   // 2:40 horas

                // Perros medianos
                (EnumTamaño.Mediano, EnumTipoPelaje.Corto) => 120,
                (EnumTamaño.Mediano, EnumTipoPelaje.SemiLargo) => 150,   // 2.5 horas
                (EnumTamaño.Mediano, EnumTipoPelaje.Largo) => 150,       // 2.5 horas
                (EnumTamaño.Mediano, EnumTipoPelaje.DobleCapa) => 180,   // 3 horas

                // Perros grandes
                (EnumTamaño.Grande, EnumTipoPelaje.Corto) => 180,        // 3 horas
                (EnumTamaño.Grande, EnumTipoPelaje.SemiLargo) => 210,    // 3.5 horas
                (EnumTamaño.Grande, EnumTipoPelaje.Largo) => 210,        // 3.5 horas
                (EnumTamaño.Grande, EnumTipoPelaje.DobleCapa) => 240,    // 4 horas

                // Default: pelo corto según tamaño
                (EnumTamaño.Pequeno, _) => 60,
                (EnumTamaño.Mediano, _) => 120,
                (EnumTamaño.Grande, _) => 180,
                _ => 60
            };
        }

        private async Task<decimal> ObtenerMultiplicadorTamañoAsync(EnumTamaño tamaño, EnumTipoPelaje tipoPelaje)
        {
            // Lógica actualizada:
            // - Pequeño pelo corto/largo/semi largo: -10% (descuento)
            // - Pequeño doble capa: 0%
            // - Mediano: 20% (todos los tipos de pelaje)
            // - Grande: 50% (todos los tipos de pelaje)

            string clave = (tamaño, tipoPelaje) switch
            {
                (EnumTamaño.Pequeno, EnumTipoPelaje.Corto) => "MULT_TAMANO_PEQUENO_CORTO",
                (EnumTamaño.Pequeno, EnumTipoPelaje.SemiLargo) => "MULT_TAMANO_PEQUENO_SEMI_LARGO",
                (EnumTamaño.Pequeno, EnumTipoPelaje.Largo) => "MULT_TAMANO_PEQUENO_LARGO",
                (EnumTamaño.Pequeno, EnumTipoPelaje.DobleCapa) => "MULT_TAMANO_PEQUENO_DOBLE_CAPA",
                (EnumTamaño.Mediano, _) => "MULT_TAMANO_MEDIANO",
                (EnumTamaño.Grande, _) => "MULT_TAMANO_GRANDE",
                _ => "MULT_TAMANO_PEQUENO_DOBLE_CAPA"
            };

            var config = await _configRepo.ObtenerPorClaveAsync(clave);
            return config?.Valor ?? 0m;
        }

        private async Task<decimal> ObtenerMultiplicadorPelajeAsync(EnumTamaño tamaño, EnumTipoPelaje tipoPelaje)
        {
            // Lógica actualizada de pelaje:
            // - Pequeño: 0% (el descuento ya está en el tamaño)
            // - Mediano pelo corto/semi-largo: 0%
            // - Mediano pelo largo: 20%
            // - Mediano doble capa: 40%
            // - Grande pelo corto: 0%
            // - Grande pelo semi-largo: 0%
            // - Grande pelo largo: 40%
            // - Grande doble capa: 50%

            string clave = (tamaño, tipoPelaje) switch
            {
                // Pequeños: 0% (descuento ya aplicado en tamaño)
                (EnumTamaño.Pequeno, _) => "MULT_PELAJE_PEQUENO",

                // Medianos: nueva lógica
                (EnumTamaño.Mediano, EnumTipoPelaje.Corto) => "MULT_PELAJE_MEDIANO_CORTO",
                (EnumTamaño.Mediano, EnumTipoPelaje.SemiLargo) => "MULT_PELAJE_MEDIANO_SEMI_LARGO",
                (EnumTamaño.Mediano, EnumTipoPelaje.Largo) => "MULT_PELAJE_MEDIANO_LARGO",
                (EnumTamaño.Mediano, EnumTipoPelaje.DobleCapa) => "MULT_PELAJE_MEDIANO_DOBLE_CAPA",

                // Grandes: nueva lógica
                (EnumTamaño.Grande, EnumTipoPelaje.Corto) => "MULT_PELAJE_GRANDE_CORTO",
                (EnumTamaño.Grande, EnumTipoPelaje.SemiLargo) => "MULT_PELAJE_GRANDE_SEMI_LARGO",
                (EnumTamaño.Grande, EnumTipoPelaje.Largo) => "MULT_PELAJE_GRANDE_LARGO",
                (EnumTamaño.Grande, EnumTipoPelaje.DobleCapa) => "MULT_PELAJE_GRANDE_DOBLE_CAPA",

                _ => "MULT_PELAJE_PEQUENO"
            };

            var config = await _configRepo.ObtenerPorClaveAsync(clave);
            return config?.Valor ?? 0m;
        }

        private async Task<bool> ValidarDisponibilidadAsync(
            int idEmpleado, DateTime fecha, TimeOnly horaInicio, TimeOnly horaFin, EnumTamaño? tamañoMascota = null)
        {
            // Verificar que el empleado esté activo y no suspendido en esta fecha
            var empleado = await _empleadoRepo.ObtenerPorIdAsync(idEmpleado);
            if (empleado == null || !empleado.Activo)
                return false;

            // Verificar si el empleado está suspendido temporalmente en esta fecha
            if (empleado.InactivoDesde != default && empleado.InactivoHasta != default)
            {
                var fechaReserva = fecha.Date;
                var inactivoDesde = empleado.InactivoDesde.Date;
                var inactivoHasta = empleado.InactivoHasta.Date;

                if (fechaReserva >= inactivoDesde && fechaReserva <= inactivoHasta)
                    return false; // Empleado suspendido en esta fecha
            }

            // No permitir domingos
            if (fecha.DayOfWeek == DayOfWeek.Sunday)
                return false;

            // Validar horario de atención según día de la semana
            if (fecha.DayOfWeek == DayOfWeek.Saturday)
            {
                // Sábados: 10:00 AM - 4:00 PM
                if (horaInicio < new TimeOnly(10, 0) || horaFin > new TimeOnly(16, 0))
                    return false;
            }
            else
            {
                // Lunes a Viernes: 9:00 AM - 5:30 PM
                if (horaInicio < new TimeOnly(9, 0) || horaFin > new TimeOnly(17, 30))
                    return false;
            }

            // Verificar que el empleado no tenga otra reserva en ese horario
            // Un empleado solo puede atender UNA mascota a la vez, sin importar el tamaño
            var reservasEmpleado = await _reservaRepo.ObtenerPorEmpleadoYFechaAsync(idEmpleado, fecha);

            foreach (var reserva in reservasEmpleado)
            {
                // Si hay traslape de horario, NO está disponible
                if (HayTraslape(horaInicio, horaFin, reserva.HoraInicio, reserva.HoraFin))
                {
                    return false; // Empleado ocupado en este horario
                }
            }

            return true;
        }

        private bool HayTraslape(TimeOnly inicio1, TimeOnly fin1, TimeOnly inicio2, TimeOnly fin2)
        {
            return inicio1 < fin2 && fin1 > inicio2;
        }

        private async Task<string> GenerarCodigoReservaAsync(DateTime fecha)
        {
            // Formato: RES-YYYYMMDD-NNN
            string fechaStr = fecha.ToString("yyyyMMdd");

            // Obtener último código del día
            int secuencial = await _reservaRepo.ObtenerSiguienteSecuencialAsync(fecha);

            return $"RES-{fechaStr}-{secuencial:D3}";
        }

        private async Task<List<EmpleadoDisponibleDTO>> ObtenerEmpleadosDisponiblesAsync(
            DateTime fecha, TimeOnly horaInicio, TimeOnly horaFin, int? idEmpleadoFiltro, EnumTamaño? tamañoMascota = null)
        {
            // Obtener todos los empleados o solo el filtrado
            var empleados = await _empleadoRepo.ObtenerTodosAsync();

            if (idEmpleadoFiltro.HasValue)
            {
                empleados = empleados.Where(e => e.Id == idEmpleadoFiltro.Value);
            }

            var empleadosDisponibles = new List<EmpleadoDisponibleDTO>();

            foreach (var empleado in empleados)
            {
                // Verificar disponibilidad
                if (await ValidarDisponibilidadAsync(empleado.Id, fecha, horaInicio, horaFin, tamañoMascota))
                {
                    empleadosDisponibles.Add(new EmpleadoDisponibleDTO
                    {
                        Id = empleado.Id,
                        Nombre = empleado.Nombre,
                        Imagen = empleado.Imagen ?? string.Empty,
                        Cargo = empleado.Cargo,
                        Calificacion = 5.0m // Por ahora fijo, puede ser calculado después
                    });
                }
            }

            return empleadosDisponibles;
        }

        private static ReservaDTO MapToDTO(ReservaEntity r)
        {
            var mascota = r.ReservaMascotas?.FirstOrDefault()?.Mascota;

            // Mapear servicios desde ReservaServicios
            var serviciosDTO = r.ReservaServicios?
                .Where(rs => rs.Servicio != null)
                .Select(rs => new ServicioReservaDTO
                {
                    IdServicio = rs.IdServicio,
                    NombreServicio = rs.Servicio.Nombre,
                    PrecioUnitario = rs.PrecioUnitario
                })
                .ToList() ?? new List<ServicioReservaDTO>();

            return new ReservaDTO
            {
                Id = r.Id,
                IdCliente = r.IdCliente,
                NombreCliente = r.Cliente?.Nombre ?? string.Empty,
                EmailCliente = r.Cliente?.Email ?? string.Empty,
                IdMascota = mascota?.Id ?? 0,
                NombreMascota = mascota?.Nombre ?? string.Empty,
                RazaMascota = mascota?.Raza ?? string.Empty,
                TamañoMascota = mascota?.Tamaño.ToString() ?? string.Empty,
                IdEmpleado = r.IdEmpleado,
                NombreEmpleado = r.Empleado?.Nombre ?? string.Empty,
                ImagenEmpleado = r.Empleado?.Imagen ?? string.Empty,
                FechaReserva = r.FechaReserva,
                HoraInicio = r.HoraInicio,
                HoraFin = r.HoraFin,
                DuracionMinutos = r.DuracionMinutos,
                Estado = r.Estado.ToString(),
                CodigoReserva = r.CodigoReserva,
                PrecioTotal = r.PrecioTotal,
                Servicios = serviciosDTO,
                Detalles = r.Detalles,
                FechaCreacion = r.FechaCreacion
            };
        }

        public async Task<IEnumerable<ReservaDTO>> ObtenerReservasSinVentaAsync()
        {
            var reservas = await _reservaRepo.ObtenerReservasSinVentaAsync();
            return reservas.Select(MapToDTO);
        }
    }
}
