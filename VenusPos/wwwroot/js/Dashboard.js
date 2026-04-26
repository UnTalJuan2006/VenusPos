// ══════════════════════════════════════════════════════════════════
// DASHBOARD ADMINISTRATIVO - VenusMascotas
// ══════════════════════════════════════════════════════════════════

// ── CONSTANTES Y CONFIGURACIÓN ─────────────────────────────────────

const API_BASE = window.location.origin + '/api';
const DIAS = ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado'];
const MESES = ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'];
const HORAS_HEATMAP = ['08:00', '09:00', '10:00', '11:00', '12:00', '13:00', '14:00', '15:00', '16:00', '17:00', '18:00'];
const DIAS_SEMANA = ['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom'];

// Variables globales
let dark = false;
let uName = 'Admin';
let uEmail = 'admin@venusmascotas.com';
let uRole = 'Admin';
let uIni = 'AD';
let uId = null;
let chartIngresos = null;

// ── PROTECCIÓN DE RUTA ─────────────────────────────────────────────

function check() {
    const token = localStorage.getItem('token');
    const rol = localStorage.getItem('rol');
    if (!token || rol !== 'admin') {
        window.location.replace('/Admin/Login.html');
    }
}

check();
window.addEventListener('pageshow', check);

// ── JWT Y USUARIO ───────────────────────────────────────────────────

try {
    const tok = localStorage.getItem('token');
    if (tok) {
        const payload = JSON.parse(atob(tok.split('.')[1]));
        uName = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || uName;
        uEmail = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || uEmail;
        uRole = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || uRole;
        uId = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || null;
        uIni = uName.split(' ').map(x => x[0]).join('').substring(0, 2).toUpperCase();
    }
} catch (e) {
    console.error('Error parsing JWT:', e);
}

// Actualizar UI con datos del usuario
document.getElementById('tbAva').textContent = uIni;
document.getElementById('tbName').textContent = uName;
document.getElementById('tbRole').textContent = uRole;
document.getElementById('ddName').textContent = uName;
document.getElementById('ddEmail').textContent = uEmail;
document.getElementById('wName').textContent = uName.split(' ')[0];

// ── FECHA ACTUAL ────────────────────────────────────────────────────

const hoy = new Date();
document.getElementById('fechaHoy').textContent =
    `${DIAS[hoy.getDay()]}, ${hoy.getDate()} de ${MESES[hoy.getMonth()]} de ${hoy.getFullYear()}`;

// ── SIDEBAR Y UI ────────────────────────────────────────────────────

function toggleSidebar() {
    document.getElementById('sidebar').classList.toggle('collapsed');
}

function toggleDrop() {
    const p = document.getElementById('tbProfile');
    const d = document.getElementById('tbDrop');
    const o = d.classList.toggle('open');
    p.classList.toggle('open', o);
}

// Cerrar dropdown al hacer clic fuera
document.addEventListener('click', function (e) {
    const w = document.querySelector('.tb-profile-wrap');
    if (w && !w.contains(e.target)) {
        document.getElementById('tbDrop').classList.remove('open');
        document.getElementById('tbProfile').classList.remove('open');
    }
});

function logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('rol');
    window.location.replace('/Admin/Login.html');
}

// ── SISTEMA DE TEMAS ────────────────────────────────────────────────

const TK = 'venus_theme';
dark = localStorage.getItem(TK) === 'dark';

function applyTheme(d) {
    document.documentElement.setAttribute('data-theme', d ? 'dark' : 'light');
    document.getElementById('iconSun').style.display = d ? 'none' : 'block';
    document.getElementById('iconMoon').style.display = d ? 'block' : 'none';
    localStorage.setItem(TK, d ? 'dark' : 'light');

    // Actualizar gráficos si existen
    if (chartIngresos) {
        updateChartColors();
    }
}

function toggleTheme() {
    dark = !dark;
    applyTheme(dark);
}

applyTheme(dark);

// ── UTILIDADES ──────────────────────────────────────────────────────

function formatCurrency(value) {
    return new Intl.NumberFormat('es-CO', {
        style: 'currency',
        currency: 'COP',
        minimumFractionDigits: 0,
        maximumFractionDigits: 0
    }).format(value).replace('COP', '$').trim();
}

function formatNumber(value) {
    return new Intl.NumberFormat('es-CO').format(value);
}

function getInitials(name) {
    if (!name) return '?';
    return name.split(' ').map(x => x[0]).join('').substring(0, 2).toUpperCase();
}

function getRandomColor(index) {
    const colors = [
        'linear-gradient(135deg, #7C3AED, #A78BFA)',
        'linear-gradient(135deg, #10B981, #34D399)',
        'linear-gradient(135deg, #F59E0B, #FBBF24)',
        'linear-gradient(135deg, #3B82F6, #60A5FA)',
        'linear-gradient(135deg, #EC4899, #F472B6)',
        'linear-gradient(135deg, #8B5CF6, #A78BFA)',
    ];
    return colors[index % colors.length];
}

// ── API CALLS ───────────────────────────────────────────────────────

async function apiGet(endpoint) {
    try {
        const response = await fetch(`${API_BASE}${endpoint}`, {
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('token')}`,
                'Content-Type': 'application/json'
            }
        });
        if (!response.ok) throw new Error(`HTTP ${response.status}`);
        return await response.json();
    } catch (error) {
        console.error(`Error fetching ${endpoint}:`, error);
        return null;
    }
}

// ── DATOS DEL DASHBOARD ─────────────────────────────────────────────

let dashboardData = {
    metricas: {
        ingresosDia: 0,
        reservasDia: 0,
        clientesActivos: 0,
        mascotasRegistradas: 0
    },
    ingresos: {
        semana: [],
        efectivo: 0,
        tarjeta: 0,
        transferencia: 0
    },
    topClientes: [],
    heatmap: [],
    serviciosSolicitados: [],
    serviciosRanking: [],
    caja: {
        estado: 'Cerrada',
        total: 0,
        efectivo: 0,
        tarjeta: 0,
        transferencia: 0,
        ultimasVentas: []
    },
    agenda: [],
    equipo: [],
    actividades: []
};

// ── CARGAR DATOS DESDE API ─────────────────────────────────────────

async function cargarDashboard() {
    try {
        // Cargar todos los datos en paralelo
        const [clientes, mascotas, reservas, cajaAbierta, ventas, empleados, servicios] = await Promise.all([
            apiGet('/Cliente'),
            apiGet('/Mascota'),
            apiGet('/Reserva'),
            apiGet('/Caja/abierta'),
            apiGet('/Venta'),
            apiGet('/Empleado'),
            apiGet('/Servicio')
        ]);

        // ── MÉTRICAS BÁSICAS ────────────────────────────────────
        if (clientes) {
            dashboardData.metricas.clientesActivos = clientes.length;
        }

        if (mascotas) {
            dashboardData.metricas.mascotasRegistradas = mascotas.length;
        }

        // ── RESERVAS DEL DÍA ────────────────────────────────────
        if (reservas) {
            const hoyStr = hoy.toISOString().split('T')[0];
            const reservasDelDia = reservas.filter(r => r.fecha?.startsWith(hoyStr));
            dashboardData.metricas.reservasDia = reservasDelDia.length;

            // Procesar agenda (primeras 6 reservas del día)
            dashboardData.agenda = procesarAgenda(reservasDelDia.slice(0, 6), clientes, mascotas, servicios);

            // Actualizar badges
            const pendientes = reservasDelDia.filter(r => r.estado === 0);
            document.getElementById('badgeReservas').textContent = pendientes.length;
            document.getElementById('reservasHoy').textContent = reservasDelDia.length;
            document.getElementById('reservasPendientes').innerHTML = `<strong>${pendientes.length} pendientes</strong>`;

            // Generar heatmap con reservas reales
            dashboardData.heatmap = generarHeatmapDesdeReservas(reservas);

            // Servicios más solicitados
            dashboardData.serviciosSolicitados = calcularServiciosSolicitados(reservas, servicios);
        }

        // ── VENTAS E INGRESOS ───────────────────────────────────
        if (ventas) {
            const hoyStr = hoy.toISOString().split('T')[0];
            const ventasDelDia = ventas.filter(v => v.fecha?.startsWith(hoyStr) && v.estado === 1);

            // Ingresos del día
            dashboardData.metricas.ingresosDia = ventasDelDia.reduce((sum, v) => sum + (v.total || 0), 0);

            // Ingresos de la semana (últimos 7 días)
            dashboardData.ingresos = calcularIngresosSemana(ventas);

            // Servicios ranking
            dashboardData.serviciosRanking = calcularRankingServicios(reservas, servicios, ventas);

            // Top clientes (por mascota)
            if (clientes && mascotas) {
                dashboardData.topClientes = calcularTopClientes(clientes, reservas, ventas, mascotas);
            }
        }

        // ── CAJA DEL DÍA ────────────────────────────────────────
        if (cajaAbierta) {
            dashboardData.caja.estado = cajaAbierta.estado || 'Abierta';
            dashboardData.caja.total = cajaAbierta.totalVentas || 0;
            dashboardData.caja.efectivo = cajaAbierta.totalEfectivo || 0;
            dashboardData.caja.tarjeta = cajaAbierta.totalTarjeta || 0;
            dashboardData.caja.transferencia = cajaAbierta.totalTransferencia || 0;
            dashboardData.caja.montoApertura = cajaAbierta.montoApertura || 0;

            // Obtener últimas ventas de la caja
            const ventasCaja = await apiGet(`/Venta/caja/${cajaAbierta.id}`);
            if (ventasCaja && ventasCaja.length > 0) {
                dashboardData.caja.ultimasVentas = procesarUltimasVentas(ventasCaja.slice(-3).reverse(), reservas, mascotas);
            } else {
                dashboardData.caja.ultimasVentas = [];
            }

            // Actualizar status en topbar
            const statusEl = document.getElementById('cajaStatus');
            if (statusEl) {
                statusEl.className = 'pill-open';
                statusEl.innerHTML = '<div class="live-dot"></div>Caja abierta';
            }
        } else {
            dashboardData.caja.estado = 'Cerrada';
            dashboardData.caja.total = 0;
            dashboardData.caja.efectivo = 0;
            dashboardData.caja.tarjeta = 0;
            dashboardData.caja.transferencia = 0;
            dashboardData.caja.ultimasVentas = [];

            const statusEl = document.getElementById('cajaStatus');
            if (statusEl) {
                statusEl.className = 'pill-open';
                statusEl.style.background = '#FEF2F2';
                statusEl.style.borderColor = '#FECACA';
                statusEl.style.color = '#991B1B';
                statusEl.innerHTML = 'Caja cerrada';
            }
        }

        // ── EQUIPO ──────────────────────────────────────────────
        if (empleados) {
            dashboardData.equipo = calcularRendimientoEquipo(empleados, reservas, ventas);
        }

        // ── ACTIVIDADES RECIENTES ───────────────────────────────
        dashboardData.actividades = generarActividadesRecientes(reservas, ventas, clientes, mascotas, cajaAbierta);

        // Renderizar todo
        renderMetricas();
        renderIngresos();
        renderTopClientes();
        renderHeatmap();
        renderServiciosSolicitados();
        renderServiciosRanking();
        renderCaja();
        renderAgenda();
        renderEquipo();
        renderActividades();

    } catch (error) {
        console.error('Error cargando dashboard:', error);
        // NO generar datos de ejemplo, mostrar solo datos reales o vacíos
        renderMetricas();
        renderIngresos();
        renderTopClientes();
        renderHeatmap();
        renderServiciosSolicitados();
        renderServiciosRanking();
        renderCaja();
        renderAgenda();
        renderEquipo();
        renderActividades();
    }
}

// ── NOTA: Se eliminó generarDatosEjemplo() - Solo usamos datos reales de la API ───

// ── PROCESAMIENTO DE DATOS DESDE API ───────────────────────────────

function procesarAgenda(reservasDelDia, clientes, mascotas, servicios) {
    if (!reservasDelDia || reservasDelDia.length === 0) return [];

    return reservasDelDia
        .sort((a, b) => new Date(a.fecha) - new Date(b.fecha)) // Ordenar por hora
        .map(reserva => {
            const mascota = mascotas?.find(m => m.id === reserva.idMascota);
            const servicio = servicios?.find(s => s.id === reserva.idServicio);

            const nombreMascota = mascota?.nombre || 'Mascota';
            const raza = mascota?.raza || 'Raza';
            const nombreServicio = servicio?.nombre || 'Servicio';

            const hora = reserva.fecha ? new Date(reserva.fecha).toLocaleTimeString('es-CO', { hour: '2-digit', minute: '2-digit' }) : '00:00';

            let estadoTexto = 'pendiente';
            if (reserva.estado === 1) estadoTexto = 'confirmada';
            else if (reserva.estado === 2) estadoTexto = 'en-curso';
            else if (reserva.estado === 3) estadoTexto = 'completada';

            return {
                hora: hora,
                cliente: `${nombreMascota} - ${raza}`,
                servicio: nombreServicio,
                estado: estadoTexto
            };
        });
}

function generarHeatmapDesdeReservas(reservas) {
    const data = [];
    const ahora = new Date();
    const inicioSemana = new Date(ahora);
    inicioSemana.setDate(ahora.getDate() - ahora.getDay() + 1); // Lunes de esta semana

    // Inicializar matriz 7 días x 11 horas
    for (let d = 0; d < 7; d++) {
        for (let h = 0; h < HORAS_HEATMAP.length; h++) {
            data.push({ dia: d, hora: h, cantidad: 0 });
        }
    }

    // Contar reservas por día y hora
    reservas.forEach(reserva => {
        if (!reserva.fecha) return;

        const fechaReserva = new Date(reserva.fecha);
        const diffDias = Math.floor((fechaReserva - inicioSemana) / (1000 * 60 * 60 * 24));

        if (diffDias >= 0 && diffDias < 7) {
            const hora = fechaReserva.getHours();
            const horaIndex = hora - 8; // 8:00 es índice 0

            if (horaIndex >= 0 && horaIndex < HORAS_HEATMAP.length) {
                const cell = data.find(c => c.dia === diffDias && c.hora === horaIndex);
                if (cell) cell.cantidad++;
            }
        }
    });

    return data;
}

function calcularServiciosSolicitados(reservas, servicios) {
    if (!servicios || !reservas || reservas.length === 0) {
        return [];
    }

    const conteo = {};
    let totalReservas = 0;

    // Contar reservas por servicio
    reservas.forEach(reserva => {
        const idServicio = reserva.idServicio;
        if (idServicio) {
            conteo[idServicio] = (conteo[idServicio] || 0) + 1;
            totalReservas++;
        }
    });

    // Si no hay reservas con servicios, retornar vacío
    if (totalReservas === 0) return [];

    // Convertir a array y ordenar
    const resultado = Object.entries(conteo)
        .map(([idServicio, cantidad]) => {
            const servicio = servicios.find(s => s.id == idServicio);
            return {
                nombre: servicio?.nombre || 'Servicio',
                cantidad: cantidad,
                total: totalReservas
            };
        })
        .sort((a, b) => b.cantidad - a.cantidad)
        .slice(0, 5);

    return resultado;
}

function calcularIngresosSemana(ventas) {
    if (!ventas || ventas.length === 0) {
        return {
            semana: [],
            efectivo: 0,
            tarjeta: 0,
            transferencia: 0
        };
    }

    const resultado = {
        semana: [],
        efectivo: 0,
        tarjeta: 0,
        transferencia: 0
    };

    const ahora = new Date();
    ahora.setHours(0, 0, 0, 0);

    const diasSemana = [];

    // Generar últimos 7 días
    for (let i = 6; i >= 0; i--) {
        const fecha = new Date(ahora);
        fecha.setDate(ahora.getDate() - i);
        const nombreDia = i === 0 ? 'Hoy' : DIAS_SEMANA[(fecha.getDay() + 6) % 7];

        diasSemana.push({
            fecha: fecha.toISOString().split('T')[0],
            dia: nombreDia,
            monto: 0,
            reservas: 0
        });
    }

    // Sumar ventas confirmadas por día
    ventas.forEach(venta => {
        // Solo ventas confirmadas (estado 1)
        if (venta.estado !== 1 && venta.estado !== 'Confirmada') return;

        const fechaVenta = venta.fecha?.split('T')[0];
        if (!fechaVenta) return;

        const dia = diasSemana.find(d => d.fecha === fechaVenta);
        if (dia) {
            dia.monto += venta.total || 0;
            dia.reservas += 1;
        }

        // Sumar por método de pago (solo del día actual)
        const hoyStr = ahora.toISOString().split('T')[0];
        if (fechaVenta === hoyStr) {
            const metodoPago = venta.metodoPago;

            if (metodoPago === 0 || metodoPago === 'Efectivo') {
                resultado.efectivo += venta.total || 0;
            } else if (metodoPago === 1 || metodoPago === 'Tarjeta') {
                resultado.tarjeta += venta.total || 0;
            } else if (metodoPago === 2 || metodoPago === 'Transferencia') {
                resultado.transferencia += venta.total || 0;
            }
        }
    });

    resultado.semana = diasSemana;
    return resultado;
}

function calcularRankingServicios(reservas, servicios, ventas) {
    if (!servicios || !ventas || ventas.length === 0) {
        return [];
    }

    const ranking = {};

    // Iterar sobre ventas confirmadas
    ventas.forEach(venta => {
        if (venta.estado !== 1 && venta.estado !== 'Confirmada') return;

        // Encontrar la reserva asociada
        const reserva = reservas?.find(r => r.id === venta.idReserva);
        if (!reserva || !reserva.idServicio) return;

        const idServicio = reserva.idServicio;

        if (!ranking[idServicio]) {
            ranking[idServicio] = { cantidad: 0, ingresos: 0 };
        }

        ranking[idServicio].cantidad++;
        ranking[idServicio].ingresos += venta.total || 0;
    });

    // Convertir a array y ordenar por ingresos
    const resultado = Object.entries(ranking)
        .map(([idServicio, datos]) => {
            const servicio = servicios.find(s => s.id == idServicio);
            return {
                nombre: servicio?.nombre || 'Servicio',
                cantidad: datos.cantidad,
                ingresos: datos.ingresos
            };
        })
        .sort((a, b) => b.ingresos - a.ingresos)
        .slice(0, 5);

    return resultado;
}

function calcularTopClientes(clientes, reservas, ventas, mascotas) {
    // Calcular por cliente (sumando todas sus ventas confirmadas)
    const clientesData = clientes.map(cliente => {
        // Obtener todas las mascotas del cliente
        const mascotasCliente = mascotas?.filter(m => m.idCliente === cliente.id) || [];

        // Obtener todas las ventas confirmadas del cliente
        const ventasCliente = ventas?.filter(v => {
            // Buscar si la venta está asociada a alguna mascota del cliente
            const reserva = reservas?.find(r => r.id === v.idReserva);
            const esMascotaDelCliente = reserva && mascotasCliente.some(m => m.id === reserva.idMascota);
            return esMascotaDelCliente && (v.estado === 1 || v.estado === 'Confirmada');
        }) || [];

        const visitas = ventasCliente.length;
        const monto = ventasCliente.reduce((sum, v) => sum + (v.total || 0), 0);

        return {
            nombre: cliente.nombre || 'Cliente',
            visitas: visitas,
            mascotas: mascotasCliente.length,
            monto: monto,
            telefono: cliente.telefono || ''
        };
    });

    const resultado = clientesData
        .filter(c => c.monto > 0)
        .sort((a, b) => b.monto - a.monto)
        .slice(0, 5);

    return resultado.length > 0 ? resultado : [];
}

function procesarUltimasVentas(ventas, reservas, mascotas) {
    if (!ventas || ventas.length === 0) return [];

    return ventas.map(venta => {
        const reserva = reservas?.find(r => r.id === venta.idReserva);
        const mascota = mascotas?.find(m => m.id === reserva?.idMascota);
        const hora = venta.fecha ? new Date(venta.fecha).toLocaleTimeString('es-CO', { hour: '2-digit', minute: '2-digit' }) : '--:--';

        let descripcion = 'Venta directa';
        if (reserva && mascota) {
            descripcion = `${mascota.nombre} - Servicio`;
        } else if (reserva) {
            descripcion = 'Servicio completado';
        }

        return {
            descripcion: descripcion,
            hora: hora,
            monto: venta.total || 0
        };
    });
}

function calcularRendimientoEquipo(empleados, reservas, ventas) {
    if (!empleados || empleados.length === 0) return [];

    const hoy = new Date();
    hoy.setHours(0, 0, 0, 0);
    const hoyStr = hoy.toISOString().split('T')[0];

    // Calcular el máximo de reservas del día entre todos los empleados
    let maxReservasHoy = 0;

    const empleadosData = empleados.map(empleado => {
        const reservasEmpleado = reservas?.filter(r => r.idEmpleado === empleado.id) || [];
        const reservasHoy = reservasEmpleado.filter(r => r.fecha?.startsWith(hoyStr));

        if (reservasHoy.length > maxReservasHoy) {
            maxReservasHoy = reservasHoy.length;
        }

        const ventasEmpleado = ventas?.filter(v => {
            const reserva = reservas?.find(r => r.id === v.idReserva);
            return reserva?.idEmpleado === empleado.id && (v.estado === 1 || v.estado === 'Confirmada');
        }) || [];

        const ingresosHoy = ventas?.filter(v => {
            const reserva = reservas?.find(r => r.id === v.idReserva);
            return reserva?.idEmpleado === empleado.id &&
                   v.fecha?.startsWith(hoyStr) &&
                   (v.estado === 1 || v.estado === 'Confirmada');
        }).reduce((sum, v) => sum + (v.total || 0), 0) || 0;

        let estado = 'disponible';
        const tieneReservaEnCurso = reservasHoy.some(r => r.estado === 2);
        const tieneReservasPendientes = reservasHoy.some(r => r.estado === 0 || r.estado === 1);

        if (tieneReservaEnCurso) {
            estado = 'ocupado';
        } else if (tieneReservasPendientes) {
            estado = 'servicio';
        }

        return {
            nombre: empleado.nombre || 'Empleado',
            rol: empleado.cargo || 'Personal',
            reservasHoy: reservasHoy.length,
            reservasTotal: reservasEmpleado.length,
            ingresos: ingresosHoy,
            estado: estado,
            porcentaje: 0 // Se calculará después
        };
    });

    // Calcular porcentaje de carga de trabajo
    // Si no hay reservas hoy, usar un máximo de 10 para la escala
    const maxParaPorcentaje = maxReservasHoy > 0 ? maxReservasHoy : 10;

    empleadosData.forEach(emp => {
        emp.porcentaje = maxParaPorcentaje > 0 ? Math.min(100, (emp.reservasHoy / maxParaPorcentaje) * 100) : 0;
    });

    // Ordenar por número de reservas del día (más ocupados primero)
    return empleadosData
        .sort((a, b) => b.reservasHoy - a.reservasHoy)
        .slice(0, 4);
}

function generarActividadesRecientes(reservas, ventas, clientes, mascotas, cajaAbierta) {
    const actividades = [];

    // Últimas 3 reservas
    if (reservas) {
        const ultimasReservas = [...reservas]
            .sort((a, b) => new Date(b.fecha) - new Date(a.fecha))
            .slice(0, 2);

        ultimasReservas.forEach(reserva => {
            const mascota = mascotas?.find(m => m.id === reserva.idMascota);
            const cliente = clientes?.find(c => c.id === reserva.idCliente);
            const hora = new Date(reserva.fecha).toLocaleTimeString('es-CO', { hour: '2-digit', minute: '2-digit' });

            actividades.push({
                tipo: 'reserva',
                titulo: `Nueva reserva - ${mascota?.nombre || 'Mascota'}`,
                desc: `${cliente?.nombre || 'Cliente'} · ${mascota?.raza || 'Raza'}`,
                hora: hora
            });
        });
    }

    // Últimas 2 ventas
    if (ventas) {
        const ultimasVentas = [...ventas]
            .filter(v => v.estado === 1)
            .sort((a, b) => new Date(b.fecha) - new Date(a.fecha))
            .slice(0, 2);

        ultimasVentas.forEach(venta => {
            const hora = new Date(venta.fecha).toLocaleTimeString('es-CO', { hour: '2-digit', minute: '2-digit' });
            const metodoPago = venta.metodoPago === 0 ? 'Efectivo' : venta.metodoPago === 1 ? 'Tarjeta' : 'Transferencia';

            actividades.push({
                tipo: 'pago',
                titulo: `Pago recibido - ${formatCurrency(venta.total)}`,
                desc: `${metodoPago}`,
                hora: hora
            });
        });
    }

    // Caja abierta
    if (cajaAbierta) {
        const horaApertura = new Date(cajaAbierta.fechaApertura).toLocaleTimeString('es-CO', { hour: '2-digit', minute: '2-digit' });
        actividades.push({
            tipo: 'caja',
            titulo: 'Caja abierta',
            desc: `Monto inicial: ${formatCurrency(cajaAbierta.montoInicial || 0)}`,
            hora: horaApertura
        });
    }

    return actividades.slice(0, 5);
}

// ── RENDER FUNCIONES ────────────────────────────────────────────────

function renderMetricas() {
    const m = dashboardData.metricas;
    document.getElementById('ingresosDia').textContent = formatCurrency(m.ingresosDia);
    document.getElementById('reservasDia').textContent = formatNumber(m.reservasDia);
    document.getElementById('clientesActivos').textContent = formatNumber(m.clientesActivos);
    document.getElementById('mascotasRegistradas').textContent = formatNumber(m.mascotasRegistradas);
}

function renderIngresos() {
    const ctx = document.getElementById('chartIngresos');
    if (!ctx) return;

    if (!dashboardData.ingresos.semana || dashboardData.ingresos.semana.length === 0) {
        // Mostrar gráfico vacío
        dashboardData.ingresos.semana = [];
        for (let i = 6; i >= 0; i--) {
            const fecha = new Date();
            fecha.setDate(fecha.getDate() - i);
            dashboardData.ingresos.semana.push({
                dia: i === 0 ? 'Hoy' : DIAS_SEMANA[(fecha.getDay() + 6) % 7],
                monto: 0
            });
        }
    }

    const labels = dashboardData.ingresos.semana.map(d => d.dia);
    const data = dashboardData.ingresos.semana.map(d => d.monto / 1000); // En miles

    const total = dashboardData.ingresos.semana.reduce((sum, d) => sum + d.monto, 0);
    const totalReservas = dashboardData.ingresos.semana.reduce((sum, d) => sum + (d.reservas || 0), 0);

    // Actualizar totales en el hero
    const ingValEl = document.querySelector('.ing-val');
    const ingCountEl = document.querySelector('.ing-count');
    const ingTrendEl = document.querySelector('.ing-trend-badge');

    if (ingValEl) ingValEl.textContent = formatCurrency(total);
    if (ingCountEl) ingCountEl.textContent = totalReservas;
    if (ingTrendEl && total > 0) {
        // Calcular tendencia comparando con la semana anterior (simplificado)
        ingTrendEl.textContent = '↑ Ventas de la semana';
    }

    // Calcular porcentajes de métodos de pago
    const totalPagos = dashboardData.ingresos.efectivo + dashboardData.ingresos.tarjeta + dashboardData.ingresos.transferencia;

    let porcEfectivo = 0, porcTarjeta = 0, porcTransferencia = 0;
    if (totalPagos > 0) {
        porcEfectivo = Math.round((dashboardData.ingresos.efectivo / totalPagos) * 100);
        porcTarjeta = Math.round((dashboardData.ingresos.tarjeta / totalPagos) * 100);
        porcTransferencia = Math.round((dashboardData.ingresos.transferencia / totalPagos) * 100);
    }

    // Actualizar leyenda de métodos de pago
    const pmLegendEl = document.querySelector('.pm-legend');
    if (pmLegendEl) {
        pmLegendEl.innerHTML = `
            <span class="pm-leg-item"><span class="pm-dot" style="background:#10B981"></span>Efectivo ${porcEfectivo}%</span>
            <span class="pm-leg-item"><span class="pm-dot" style="background:#3B82F6"></span>Tarjeta ${porcTarjeta}%</span>
            <span class="pm-leg-item"><span class="pm-dot" style="background:#8B5CF6"></span>Transferencia ${porcTransferencia}%</span>
        `;
    }

    // Actualizar barra combinada
    const pmBarEl = document.querySelector('.pm-bar-combined');
    if (pmBarEl) {
        pmBarEl.innerHTML = `
            <div style="width:${porcEfectivo}%;background:#10B981" title="Efectivo ${porcEfectivo}%"></div>
            <div style="width:${porcTarjeta}%;background:#3B82F6" title="Tarjeta ${porcTarjeta}%"></div>
            <div style="width:${porcTransferencia}%;background:#8B5CF6" title="Transferencia ${porcTransferencia}%"></div>
        `;
    }

    // Destruir gráfico anterior si existe
    if (chartIngresos) {
        chartIngresos.destroy();
    }

    chartIngresos = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Ingresos',
                data: data,
                borderColor: '#7C3AED',
                backgroundColor: 'rgba(124, 58, 237, 0.1)',
                borderWidth: 3,
                fill: true,
                tension: 0.4,
                pointRadius: 5,
                pointBackgroundColor: '#7C3AED',
                pointBorderColor: '#fff',
                pointBorderWidth: 2,
                pointHoverRadius: 7
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: {
                    callbacks: {
                        label: (ctx) => `$${ctx.parsed.y.toFixed(1)}k`
                    }
                }
            },
            scales: {
                x: {
                    grid: { display: false },
                    ticks: {
                        font: { family: 'DM Sans', weight: '700', size: 11 },
                        color: dark ? '#7060A0' : '#8876AE'
                    }
                },
                y: {
                    beginAtZero: true,
                    grid: { color: dark ? '#2A2250' : '#E8E3F5' },
                    ticks: {
                        font: { family: 'DM Sans', size: 11 },
                        color: dark ? '#7060A0' : '#8876AE',
                        callback: (value) => `$${value}k`
                    }
                }
            }
        }
    });
}

function renderTopClientes() {
    const container = document.getElementById('topClientesList');
    if (!container) return;

    if (!dashboardData.topClientes || dashboardData.topClientes.length === 0) {
        container.innerHTML = '<div style="padding: 20px; text-align: center; color: #6B7280;">No hay datos de clientes con compras</div>';
        return;
    }

    container.innerHTML = dashboardData.topClientes.map((cliente, idx) => {
        const rankClass = idx === 0 ? 'top1' : idx === 1 ? 'top2' : idx === 2 ? 'top3' : '';
        const initials = getInitials(cliente.nombre);

        return `
            <div class="tc-item">
                <div class="tc-rank ${rankClass}">${idx + 1}</div>
                <div class="tc-avatar" style="background: ${getRandomColor(idx)}">${initials}</div>
                <div class="tc-info">
                    <div class="tc-name">${cliente.nombre}</div>
                    <div class="tc-stats">${cliente.visitas} compra${cliente.visitas !== 1 ? 's' : ''} · ${cliente.mascotas} mascota${cliente.mascotas !== 1 ? 's' : ''}</div>
                </div>
                <div class="tc-amount">${formatCurrency(cliente.monto)}</div>
            </div>
        `;
    }).join('');
}

function renderHeatmap() {
    const container = document.getElementById('heatmapContainer');
    if (!container) return;

    let html = '<div class="heatmap-grid">';

    // Header vacío
    html += '<div class="hm-header"></div>';

    // Headers de días
    DIAS_SEMANA.forEach(dia => {
        html += `<div class="hm-header">${dia}</div>`;
    });

    // Filas por hora
    HORAS_HEATMAP.forEach((hora, hIdx) => {
        html += `<div class="hm-time">${hora}</div>`;

        for (let d = 0; d < 7; d++) {
            const cell = dashboardData.heatmap.find(c => c.dia === d && c.hora === hIdx);
            const cantidad = cell ? cell.cantidad : 0;
            const level = cantidad === 0 ? 0 : cantidad <= 1 ? 1 : cantidad <= 2 ? 2 : cantidad <= 3 ? 3 : 4;

            html += `
                <div class="hm-cell level-${level}" title="${cantidad} reserva${cantidad !== 1 ? 's' : ''}">
                    ${cantidad || ''}
                </div>
            `;
        }
    });

    html += '</div>';
    container.innerHTML = html;
}

function renderServiciosSolicitados() {
    const container = document.getElementById('servicesList');
    if (!container) return;

    if (!dashboardData.serviciosSolicitados || dashboardData.serviciosSolicitados.length === 0) {
        container.innerHTML = '<div style="padding: 20px; text-align: center; color: #6B7280;">No hay datos de servicios solicitados</div>';
        return;
    }

    const colors = ['#7C3AED', '#F59E0B', '#10B981', '#3B82F6', '#EC4899'];

    container.innerHTML = dashboardData.serviciosSolicitados.map((servicio, idx) => {
        const porcentaje = servicio.total > 0 ? (servicio.cantidad / servicio.total) * 100 : 0;
        const porcentajeRedondeado = Math.round(porcentaje);

        return `
            <div class="sl-item">
                <div class="sl-header">
                    <div class="sl-name">${servicio.nombre}</div>
                    <div class="sl-value">${servicio.cantidad} (${porcentajeRedondeado}%)</div>
                </div>
                <div class="sl-bar-container">
                    <div class="sl-bar" style="width: ${porcentaje}%; background: ${colors[idx % colors.length]}" title="${servicio.nombre}: ${servicio.cantidad} reservas (${porcentajeRedondeado}%)"></div>
                </div>
            </div>
        `;
    }).join('');
}

function renderServiciosRanking() {
    const container = document.getElementById('serviceRanking');
    if (!container) return;

    if (!dashboardData.serviciosRanking || dashboardData.serviciosRanking.length === 0) {
        container.innerHTML = '<div style="padding: 20px; text-align: center; color: #6B7280;">No hay datos de ranking de servicios</div>';
        return;
    }

    const totalServicios = dashboardData.serviciosRanking.reduce((sum, s) => sum + s.cantidad, 0);
    const totalIngresos = dashboardData.serviciosRanking.reduce((sum, s) => sum + s.ingresos, 0);

    let html = dashboardData.serviciosRanking.map((servicio, idx) => `
        <div class="sr-item">
            <div class="sr-rank">${idx + 1}</div>
            <div class="sr-name">${servicio.nombre}</div>
            <div class="sr-stat">
                <div class="sr-label">Servicios</div>
                <div class="sr-value">${servicio.cantidad}</div>
            </div>
            <div class="sr-stat">
                <div class="sr-label">Ingresos</div>
                <div class="sr-value">${formatCurrency(servicio.ingresos)}</div>
            </div>
        </div>
    `).join('');

    html += `
        <div class="sr-total">
            <div class="sr-total-item">
                <div class="sr-total-label">Total Servicios</div>
                <div class="sr-total-value">${totalServicios}</div>
            </div>
            <div class="sr-total-item">
                <div class="sr-total-label">Total Ingresos</div>
                <div class="sr-total-value">${formatCurrency(totalIngresos)}</div>
            </div>
        </div>
    `;

    container.innerHTML = html;
}

function renderCaja() {
    const c = dashboardData.caja;

    const estadoEl = document.getElementById('estadoCaja');
    const estadoPillEl = document.getElementById('cajaEstadoPill');
    const responsableEl = document.getElementById('cajaResponsable');
    const totalEl = document.getElementById('cajaTotalAcumulado');
    const subtituloEl = document.getElementById('cajaSubtitulo');
    const efectivoEl = document.getElementById('cajaEfectivo');
    const tarjetaEl = document.getElementById('cajaTarjeta');
    const transferenciaEl = document.getElementById('cajaTransferencia');
    const ventasContainer = document.getElementById('ultimasVentasList');

    if (estadoEl) estadoEl.textContent = `Estado: ${c.estado}`;
    if (estadoPillEl) estadoPillEl.textContent = `● ${c.estado}`;
    if (responsableEl) responsableEl.textContent = c.estado === 'Abierta' ? 'Caja abierta' : 'Caja cerrada';
    if (totalEl) totalEl.textContent = formatCurrency(c.total);
    if (subtituloEl) {
        const numVentas = c.ultimasVentas ? c.ultimasVentas.length : 0;
        subtituloEl.textContent = numVentas > 0 ? `acumulado · ${numVentas} venta${numVentas !== 1 ? 's' : ''}` : 'Sin ventas';
    }
    if (efectivoEl) efectivoEl.textContent = formatCurrency(c.efectivo);
    if (tarjetaEl) tarjetaEl.textContent = formatCurrency(c.tarjeta);
    if (transferenciaEl) transferenciaEl.textContent = formatCurrency(c.transferencia);

    if (ventasContainer) {
        if (c.ultimasVentas && c.ultimasVentas.length > 0) {
            ventasContainer.innerHTML = c.ultimasVentas.map(venta => `
                <div class="uv-item">
                    <div class="uv-info">
                        <div class="uv-desc">${venta.descripcion}</div>
                        <div class="uv-time">${venta.hora}</div>
                    </div>
                    <div class="uv-amount">${formatCurrency(venta.monto)}</div>
                </div>
            `).join('');
        } else {
            ventasContainer.innerHTML = '<div style="padding: 20px; text-align: center; color: #6B7280;">No hay ventas registradas</div>';
        }
    }
}

function renderAgenda() {
    const container = document.getElementById('agendaList');
    if (!container) return;

    if (!dashboardData.agenda || dashboardData.agenda.length === 0) {
        container.innerHTML = '<div style="padding: 20px; text-align: center; color: #6B7280;">No hay reservas para hoy</div>';
        return;
    }

    container.innerHTML = dashboardData.agenda.map(item => `
        <div class="ag-item">
            <div class="ag-time">${item.hora || '00:00'}</div>
            <div class="ag-info">
                <div class="ag-client">${item.cliente || 'Cliente'}</div>
                <div class="ag-service">${item.servicio || 'Servicio'}</div>
            </div>
            <div class="ag-status ${item.estado || 'pendiente'}">${
                item.estado === 'confirmada' ? 'Confirmada' :
                item.estado === 'en-curso' ? 'En curso' :
                item.estado === 'completada' ? 'Completada' : 'Pendiente'
            }</div>
        </div>
    `).join('');
}

function renderEquipo() {
    const container = document.getElementById('teamRanking');
    if (!container) return;

    if (!dashboardData.equipo || dashboardData.equipo.length === 0) {
        container.innerHTML = '<div style="padding: 20px; text-align: center; color: #6B7280;">No hay datos de empleados</div>';
        return;
    }

    container.innerHTML = dashboardData.equipo.map(empleado => {
        const initials = getInitials(empleado.nombre);
        const porcentaje = empleado.porcentaje || 0;

        // Color de la barra según la carga de trabajo
        let barColor = '#10B981'; // Verde (baja carga)
        if (porcentaje > 70) barColor = '#EF4444'; // Rojo (alta carga)
        else if (porcentaje > 40) barColor = '#F59E0B'; // Naranja (media carga)

        return `
            <div class="tr-item">
                <div class="tr-avatar">${initials}</div>
                <div class="tr-info">
                    <div class="tr-name">${empleado.nombre}</div>
                    <div class="tr-role">${empleado.rol}</div>
                </div>
                <div class="tr-stat">
                    <div class="tr-stat-value">${empleado.reservasHoy || 0}</div>
                    <div class="tr-stat-label">Hoy</div>
                </div>
                <div class="tr-stat" style="flex: 1; min-width: 120px;">
                    <div class="tr-stat-label" style="margin-bottom: 4px;">Carga: ${Math.round(porcentaje)}%</div>
                    <div style="background: #E5E7EB; border-radius: 4px; height: 8px; overflow: hidden;">
                        <div style="background: ${barColor}; height: 100%; width: ${porcentaje}%; transition: width 0.3s;"></div>
                    </div>
                </div>
                <div class="tr-stat">
                    <div class="tr-stat-value">${formatCurrency(empleado.ingresos)}</div>
                    <div class="tr-stat-label">Ingresos Hoy</div>
                </div>
                <div class="tr-status ${empleado.estado}">${
                    empleado.estado === 'disponible' ? 'Disponible' :
                    empleado.estado === 'servicio' ? 'Con reservas' : 'Ocupado'
                }</div>
            </div>
        `;
    }).join('');
}

function renderActividades() {
    const container = document.getElementById('activityList');
    if (!container) return;

    const iconos = {
        reserva: '📅',
        completado: '✅',
        pago: '💰',
        mascota: '🐾',
        caja: '💵',
        cliente: '👤',
        cancelacion: '❌'
    };

    container.innerHTML = dashboardData.actividades.map(act => `
        <div class="act-item">
            <div class="act-icon">${iconos[act.tipo] || '📋'}</div>
            <div class="act-content">
                <div class="act-title">${act.titulo}</div>
                <div class="act-desc">${act.desc}</div>
            </div>
            <div class="act-time">${act.hora}</div>
        </div>
    `).join('');
}

function renderAll() {
    renderMetricas();
    renderIngresos();
    renderTopClientes();
    renderHeatmap();
    renderServiciosSolicitados();
    renderServiciosRanking();
    renderCaja();
    renderAgenda();
    renderEquipo();
    renderActividades();
}

// ── ACTUALIZAR COLORES DE GRÁFICOS ─────────────────────────────────

function updateChartColors() {
    if (!chartIngresos) return;

    const gridColor = dark ? '#2A2250' : '#E8E3F5';
    const textColor = dark ? '#7060A0' : '#8876AE';

    chartIngresos.options.scales.x.ticks.color = textColor;
    chartIngresos.options.scales.y.ticks.color = textColor;
    chartIngresos.options.scales.y.grid.color = gridColor;
    chartIngresos.update();
}

// ── INICIALIZACIÓN ──────────────────────────────────────────────────

document.addEventListener('DOMContentLoaded', () => {
    cargarDashboard();

    // Actualizar cada 5 minutos
    setInterval(cargarDashboard, 5 * 60 * 1000);
});
