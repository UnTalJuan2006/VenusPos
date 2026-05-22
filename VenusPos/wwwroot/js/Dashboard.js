// ══════════════════════════════════════════════════════════════════
// DASHBOARD ADMINISTRATIVO - VenusMascotas
// ══════════════════════════════════════════════════════════════════

// ── CONSTANTES Y CONFIGURACIÓN ─────────────────────────────────────

const API_BASE = window.location.origin + '/api';
const DIAS = ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado'];
const MESES = ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'];
const HORAS_HEATMAP = ['08:00', '09:00', '10:00', '11:00', '12:00', '13:00', '14:00', '15:00', '16:00', '17:00', '18:00'];
const DIAS_SEMANA = ['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom'];

// Configuración de caché
const CACHE_KEYS = {
    CLIENTES: 'venus_cache_clientes',
    MASCOTAS: 'venus_cache_mascotas',
    RESERVAS: 'venus_cache_reservas',
    VENTAS: 'venus_cache_ventas',
    EMPLEADOS: 'venus_cache_empleados',
    SERVICIOS: 'venus_cache_servicios',
    TIMESTAMP: 'venus_cache_timestamp'
};

const CACHE_DURATION = 3 * 60 * 1000; // 3 minutos
const BACKGROUND_REFRESH = 30 * 1000; // 30 segundos para actualizaciones en background

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

// ── SISTEMA DE CACHÉ ───────────────────────────────────────────────

function getCachedData(key) {
    try {
        const cached = localStorage.getItem(key);
        return cached ? JSON.parse(cached) : null;
    } catch (error) {
        console.error(`Error reading cache ${key}:`, error);
        return null;
    }
}

function setCachedData(key, data) {
    try {
        localStorage.setItem(key, JSON.stringify(data));
    } catch (error) {
        console.error(`Error writing cache ${key}:`, error);
    }
}

function isCacheValid() {
    const timestamp = localStorage.getItem(CACHE_KEYS.TIMESTAMP);
    if (!timestamp) return false;

    const now = Date.now();
    const cacheAge = now - parseInt(timestamp, 10);
    return cacheAge < CACHE_DURATION;
}

function updateCacheTimestamp() {
    localStorage.setItem(CACHE_KEYS.TIMESTAMP, Date.now().toString());
}

// ── API CALLS CON CACHÉ ─────────────────────────────────────────────

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

async function apiGetWithCache(endpoint, cacheKey) {
    // Intentar cargar desde caché primero
    const cached = getCachedData(cacheKey);

    // Si hay caché válida, usarla
    if (cached && isCacheValid()) {
        console.log(`✓ Usando caché para ${endpoint}`);

        // Actualizar en background si el caché tiene más de 30 segundos
        const timestamp = localStorage.getItem(CACHE_KEYS.TIMESTAMP);
        const cacheAge = Date.now() - parseInt(timestamp, 10);

        if (cacheAge > BACKGROUND_REFRESH) {
            console.log(`🔄 Actualizando ${endpoint} en background...`);
            apiGet(endpoint).then(data => {
                if (data) {
                    setCachedData(cacheKey, data);
                    updateCacheTimestamp();
                }
            });
        }

        return cached;
    }

    // Si no hay caché válida, hacer petición
    console.log(`🌐 Cargando ${endpoint} desde API...`);
    const data = await apiGet(endpoint);

    if (data) {
        setCachedData(cacheKey, data);
        updateCacheTimestamp();
    }

    return data;
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

async function cargarDashboard(forzarRecarga = false) {
    try {
        // Mostrar indicador de carga si es forzado
        if (forzarRecarga) {
            mostrarIndicadorCarga(true);
        }

        // Si forzamos recarga, limpiar caché
        if (forzarRecarga) {
            limpiarCache();
        }

        // Cargar datos con sistema de caché inteligente
        const [clientes, mascotas, reservas, cajaAbierta, ventas, empleados, servicios, serviciosVendidos] = await Promise.all([
            apiGetWithCache('/Cliente', CACHE_KEYS.CLIENTES),
            apiGetWithCache('/Mascota', CACHE_KEYS.MASCOTAS),
            apiGetWithCache('/Reserva', CACHE_KEYS.RESERVAS),
            apiGet('/Caja/abierta'), // No cachear la caja, siempre actualizada
            apiGetWithCache('/Venta', CACHE_KEYS.VENTAS),
            apiGetWithCache('/Empleado', CACHE_KEYS.EMPLEADOS),
            apiGetWithCache('/Servicio', CACHE_KEYS.SERVICIOS),
            apiGet('/Venta/servicios-mas-vendidos?top=5') // No cachear estadísticas dinámicas
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
            // Obtener fecha local en formato YYYY-MM-DD (corregido para zona horaria local)
            const hoyLocal = new Date(hoy.getFullYear(), hoy.getMonth(), hoy.getDate());
            const hoyStr = `${hoyLocal.getFullYear()}-${String(hoyLocal.getMonth() + 1).padStart(2, '0')}-${String(hoyLocal.getDate()).padStart(2, '0')}`;

            console.log('📅 Fecha de hoy (Local):', hoyStr);
            console.log('📅 Total de reservas:', reservas.length);

            const reservasDelDia = reservas.filter(r => {
                // Usar FechaReserva en lugar de fecha
                if (!r.fechaReserva) {
                    console.log('⚠️ Reserva sin fechaReserva:', r);
                    return false;
                }

                // Obtener solo la parte de fecha (YYYY-MM-DD)
                const fechaReserva = r.fechaReserva.split('T')[0];
                const coincide = fechaReserva === hoyStr;

                if (coincide) {
                    console.log('✅ Reserva del día encontrada:', r.id, 'Fecha:', fechaReserva);
                }

                return coincide;
            });

            dashboardData.metricas.reservasDia = reservasDelDia.length;
            console.log('✅ Reservas del día:', reservasDelDia.length, 'de un total de', reservas.length);

            if (reservasDelDia.length > 0) {
                console.log('📋 Reservas del día:', reservasDelDia);
            }

            // Procesar agenda (TODAS las reservas del día actual)
            dashboardData.agenda = procesarAgenda(reservasDelDia, clientes, mascotas, servicios, empleados);
            console.log('✅ Agenda procesada:', dashboardData.agenda.length, 'reservas del día actual');

            // Actualizar badges
            // Estado es string: "Pendiente", "Confirmada", "EnCurso", "Completada", "Cancelada"
            const pendientes = reservasDelDia.filter(r => r.estado === 'Pendiente');
            const badgeEl = document.getElementById('badgeReservas');
            const reservasHoyEl = document.getElementById('reservasHoy');
            const reservasPendientesEl = document.getElementById('reservasPendientes');

            if (badgeEl) badgeEl.textContent = pendientes.length;
            if (reservasHoyEl) reservasHoyEl.textContent = reservasDelDia.length;
            if (reservasPendientesEl) reservasPendientesEl.innerHTML = `<strong>${pendientes.length} pendiente${pendientes.length !== 1 ? 's' : ''}</strong>`;

            // Generar heatmap con reservas reales
            dashboardData.heatmap = generarHeatmapDesdeReservas(reservas);

            // Servicios más solicitados
            dashboardData.serviciosSolicitados = calcularServiciosSolicitados(reservas, servicios);
        }

        // ── VENTAS E INGRESOS ───────────────────────────────────
        if (ventas) {
            const hoyLocal = new Date(hoy.getFullYear(), hoy.getMonth(), hoy.getDate());
            const hoyStr = `${hoyLocal.getFullYear()}-${String(hoyLocal.getMonth() + 1).padStart(2, '0')}-${String(hoyLocal.getDate()).padStart(2, '0')}`;
            const ventasDelDia = ventas.filter(v => {
                const fechaVenta = (v.fechaVenta || v.fecha);
                return fechaVenta?.startsWith(hoyStr) && (v.estado === 1 || v.estado === 'Confirmada');
            });

            // Ingresos del día
            dashboardData.metricas.ingresosDia = ventasDelDia.reduce((sum, v) => sum + (v.total || 0), 0);
            console.log('✅ Ingresos del día:', formatCurrency(dashboardData.metricas.ingresosDia), 'de', ventasDelDia.length, 'ventas');

            // Ingresos de la semana (últimos 7 días)
            dashboardData.ingresos = calcularIngresosSemana(ventas);

            // Servicios ranking (rendimiento de servicios) - Usar datos del endpoint
            if (serviciosVendidos && serviciosVendidos.length > 0) {
                dashboardData.serviciosRanking = serviciosVendidos.map(sv => ({
                    nombre: sv.nombreServicio,
                    cantidad: sv.cantidadVendida,
                    ingresos: sv.totalIngresos
                }));
                console.log('✅ Servicios Ranking cargados desde API:', dashboardData.serviciosRanking);
            } else {
                dashboardData.serviciosRanking = [];
                console.log('⚠️ No hay servicios vendidos disponibles');
            }

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
        dashboardData.actividades = generarActividadesRecientes(reservas, ventas, clientes, mascotas, cajaAbierta, empleados, servicios);

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

        // Ocultar indicador de carga
        mostrarIndicadorCarga(false);

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

        mostrarIndicadorCarga(false);
    }
}

// ── UTILIDADES DE CACHÉ ────────────────────────────────────────────

function limpiarCache() {
    console.log('🗑️ Limpiando caché...');
    Object.values(CACHE_KEYS).forEach(key => {
        localStorage.removeItem(key);
    });
}

function mostrarIndicadorCarga(mostrar) {
    const statusEl = document.getElementById('cajaStatus');
    if (!statusEl) return;

    if (mostrar) {
        const originalHTML = statusEl.innerHTML;
        statusEl.dataset.original = originalHTML;
        statusEl.innerHTML = '<div class="live-dot" style="animation: pulse 1.5s infinite;"></div>Actualizando...';
        statusEl.style.background = '#EFF6FF';
        statusEl.style.borderColor = '#BFDBFE';
        statusEl.style.color = '#1E40AF';
    } else {
        if (statusEl.dataset.original) {
            // No restaurar, dejar que se actualice naturalmente
            delete statusEl.dataset.original;
        }
    }
}

// ── NOTA: Se eliminó generarDatosEjemplo() - Solo usamos datos reales de la API ───

// ── PROCESAMIENTO DE DATOS DESDE API ───────────────────────────────

function procesarAgenda(reservasDelDia, clientes, mascotas, servicios, empleados) {
    if (!reservasDelDia || reservasDelDia.length === 0) {
        console.log('⚠️ No hay reservas del día para procesar agenda');
        return [];
    }

    console.log('📋 Procesando agenda con', reservasDelDia.length, 'reservas');

    const agendaItems = reservasDelDia
        .sort((a, b) => {
            // Ordenar por HoraInicio
            const horaA = a.horaInicio || '00:00:00';
            const horaB = b.horaInicio || '00:00:00';
            return horaA.localeCompare(horaB);
        })
        .map(reserva => {
            // La reserva ya tiene los datos incluidos en el DTO
            const nombreMascota = reserva.nombreMascota || 'Mascota';
            const raza = reserva.razaMascota || 'Raza';
            const nombreEmpleado = reserva.nombreEmpleado || 'Sin asignar';

            // Los servicios vienen en la lista Servicios
            const serviciosNombres = reserva.servicios && reserva.servicios.length > 0
                ? reserva.servicios.map(s => s.nombre || s.nombreServicio).join(', ')
                : 'Servicio';

            // Formatear hora desde HoraInicio (formato TimeOnly "HH:mm:ss")
            const hora = reserva.horaInicio
                ? reserva.horaInicio.substring(0, 5) // Tomar solo HH:mm
                : '00:00';

            // Estado es string: "Pendiente", "Confirmada", "EnCurso", "Completada", "Cancelada"
            let estadoTexto = 'pendiente';
            if (reserva.estado === 'Confirmada') estadoTexto = 'confirmada';
            else if (reserva.estado === 'EnCurso') estadoTexto = 'en-curso';
            else if (reserva.estado === 'Completada') estadoTexto = 'completada';
            else if (reserva.estado === 'Cancelada') estadoTexto = 'cancelada';

            return {
                hora: hora,
                cliente: `${nombreMascota} - ${raza}`,
                servicio: serviciosNombres,
                groomer: nombreEmpleado,
                estado: estadoTexto
            };
        });

    console.log('✅ Items de agenda procesados:', agendaItems);
    return agendaItems;
}

function generarHeatmapDesdeReservas(reservas) {
    if (!reservas || reservas.length === 0) {
        console.log('No hay reservas para generar heatmap');
        return [];
    }

    const data = [];
    const ahora = new Date();
    const inicioSemana = new Date(ahora);
    inicioSemana.setDate(ahora.getDate() - ahora.getDay() + 1); // Lunes de esta semana
    inicioSemana.setHours(0, 0, 0, 0);

    // Inicializar matriz 7 días x 11 horas
    for (let d = 0; d < 7; d++) {
        for (let h = 0; h < HORAS_HEATMAP.length; h++) {
            data.push({ dia: d, hora: h, cantidad: 0 });
        }
    }

    console.log('Generando heatmap desde', inicioSemana, 'con', reservas.length, 'reservas');

    // Contar reservas por día y hora
    let contadas = 0;
    reservas.forEach(reserva => {
        if (!reserva.fechaReserva) return;

        const fechaReserva = new Date(reserva.fechaReserva);
        const diffDias = Math.floor((fechaReserva - inicioSemana) / (1000 * 60 * 60 * 24));

        if (diffDias >= 0 && diffDias < 7) {
            // Obtener la hora desde horaInicio (formato "HH:mm:ss")
            let hora = 0;
            if (reserva.horaInicio) {
                const horaStr = reserva.horaInicio.split(':')[0];
                hora = parseInt(horaStr, 10);
            }
            const horaIndex = hora - 8; // 8:00 es índice 0

            if (horaIndex >= 0 && horaIndex < HORAS_HEATMAP.length) {
                const cell = data.find(c => c.dia === diffDias && c.hora === horaIndex);
                if (cell) {
                    cell.cantidad++;
                    contadas++;
                }
            }
        }
    });

    console.log('Heatmap generado:', contadas, 'reservas contadas');
    return data;
}

function calcularServiciosSolicitados(reservas, servicios) {
    if (!reservas || reservas.length === 0) {
        return [];
    }

    const conteo = {};
    let totalServicios = 0;

    // Contar servicios en todas las reservas
    reservas.forEach(reserva => {
        // Cada reserva tiene una lista de servicios
        if (reserva.servicios && reserva.servicios.length > 0) {
            reserva.servicios.forEach(servicio => {
                const nombreServicio = servicio.nombre || servicio.nombreServicio || 'Servicio';
                if (!conteo[nombreServicio]) {
                    conteo[nombreServicio] = 0;
                }
                conteo[nombreServicio]++;
                totalServicios++;
            });
        }
    });

    // Si no hay servicios, retornar vacío
    if (totalServicios === 0) return [];

    // Convertir a array y ordenar
    const resultado = Object.entries(conteo)
        .map(([nombre, cantidad]) => {
            return {
                nombre: nombre,
                cantidad: cantidad,
                total: totalServicios
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
        // Solo ventas confirmadas (estado 1 o "Confirmada")
        if (venta.estado !== 1 && venta.estado !== 'Confirmada') return;

        // Usar fechaVenta en lugar de fecha
        const fechaVentaCompleta = venta.fechaVenta || venta.fecha;
        if (!fechaVentaCompleta) return;

        const fechaVenta = fechaVentaCompleta.split('T')[0];

        const dia = diasSemana.find(d => d.fecha === fechaVenta);
        if (dia) {
            dia.monto += venta.total || 0;
            dia.reservas += 1;
        }

        // Sumar por método de pago (solo del día actual)
        const hoyLocal = new Date(ahora.getFullYear(), ahora.getMonth(), ahora.getDate());
        const hoyStr = `${hoyLocal.getFullYear()}-${String(hoyLocal.getMonth() + 1).padStart(2, '0')}-${String(hoyLocal.getDate()).padStart(2, '0')}`;
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

function calcularRankingServicios(reservas, servicios) {
    // SIMPLIFICADO: Solo contar cuántas veces se eligió cada servicio
    if (!servicios || servicios.length === 0) {
        console.log('⚠️ No hay servicios disponibles');
        return [];
    }

    if (!reservas || reservas.length === 0) {
        console.log('⚠️ No hay reservas disponibles para ranking');
        return [];
    }

    const conteoServicios = {};

    // Contar cuántas veces se eligió cada servicio en las reservas
    reservas.forEach(reserva => {
        if (reserva.idServicio) {
            if (!conteoServicios[reserva.idServicio]) {
                conteoServicios[reserva.idServicio] = 0;
            }
            conteoServicios[reserva.idServicio]++;
        }
    });

    console.log('📊 Conteo de servicios:', conteoServicios);

    // Convertir a array y ordenar por cantidad
    const resultado = Object.entries(conteoServicios)
        .map(([idServicio, cantidad]) => {
            const servicio = servicios.find(s => s.id == idServicio);
            return {
                nombre: servicio?.nombre || 'Servicio',
                cantidad: cantidad,
                ingresos: 0 // No mostrar ingresos, solo cantidad
            };
        })
        .sort((a, b) => b.cantidad - a.cantidad)
        .slice(0, 5);

    console.log('✅ Ranking de servicios:', resultado);
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
        const fechaVenta = venta.fechaVenta || venta.fecha;
        const hora = fechaVenta ? new Date(fechaVenta).toLocaleTimeString('es-CO', { hour: '2-digit', minute: '2-digit' }) : '--:--';

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
    if (!empleados || empleados.length === 0) {
        console.log('⚠️ No hay empleados disponibles');
        return [];
    }

    const hoy = new Date();
    hoy.setHours(0, 0, 0, 0);
    const hoyLocal = new Date(hoy.getFullYear(), hoy.getMonth(), hoy.getDate());
    const hoyStr = `${hoyLocal.getFullYear()}-${String(hoyLocal.getMonth() + 1).padStart(2, '0')}-${String(hoyLocal.getDate()).padStart(2, '0')}`;
    console.log('📅 Calculando rendimiento de equipo para:', hoyStr);

    // Calcular el máximo de reservas del día entre todos los empleados
    let maxReservasHoy = 0;

    const empleadosData = empleados.map(empleado => {
        const reservasEmpleado = reservas?.filter(r => r.idEmpleado === empleado.id) || [];
        const reservasHoy = reservasEmpleado.filter(r => {
            if (!r.fechaReserva) return false;
            return r.fechaReserva.startsWith(hoyStr);
        });

        if (reservasHoy.length > maxReservasHoy) {
            maxReservasHoy = reservasHoy.length;
        }

        const ventasEmpleado = ventas?.filter(v => {
            const reserva = reservas?.find(r => r.id === v.idReserva);
            return reserva?.idEmpleado === empleado.id && (v.estado === 1 || v.estado === 'Confirmada');
        }) || [];

        const ingresosHoy = ventas?.filter(v => {
            const reserva = reservas?.find(r => r.id === v.idReserva);
            const fechaVenta = v.fechaVenta || v.fecha;
            return reserva?.idEmpleado === empleado.id &&
                   fechaVenta?.startsWith(hoyStr) &&
                   (v.estado === 1 || v.estado === 'Confirmada');
        }).reduce((sum, v) => sum + (v.total || 0), 0) || 0;

        let estado = 'disponible';
        // Estado es string: "Pendiente", "Confirmada", "EnCurso", "Completada", "Cancelada"
        const tieneReservaEnCurso = reservasHoy.some(r => r.estado === 'EnCurso');
        const tieneReservasPendientes = reservasHoy.some(r => r.estado === 'Pendiente' || r.estado === 'Confirmada');

        if (tieneReservaEnCurso) {
            estado = 'ocupado';
        } else if (tieneReservasPendientes) {
            estado = 'servicio';
        }

        console.log(`👤 ${empleado.nombre}: ${reservasHoy.length} reservas hoy, ${formatCurrency(ingresosHoy)} ingresos, estado: ${estado}`);

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

    console.log('✅ Rendimiento de equipo calculado:', empleadosData.length, 'empleados');

    // Ordenar por número de reservas del día (más ocupados primero)
    return empleadosData
        .sort((a, b) => b.reservasHoy - a.reservasHoy)
        .slice(0, 4);
}

function generarActividadesRecientes(reservas, ventas, clientes, mascotas, cajaAbierta, empleados, servicios) {
    const actividades = [];
    const ahora = new Date();
    const hace24Horas = new Date(ahora.getTime() - (24 * 60 * 60 * 1000)); // 24 horas atrás
    const hace3Dias = new Date(ahora.getTime() - (3 * 24 * 60 * 60 * 1000)); // 3 días atrás (fallback)

    // Función auxiliar para verificar si una fecha es reciente
    const esReciente = (fecha, usarFallback = false) => {
        if (!fecha) return false;
        const fechaObj = new Date(fecha);
        const limite = usarFallback ? hace3Dias : hace24Horas;
        return fechaObj >= limite;
    };

    // Agregar reservas recientes
    if (reservas && reservas.length > 0) {
        const reservasRecientes = reservas
            .filter(r => r.fechaReserva && esReciente(r.fechaReserva, true))
            .sort((a, b) => new Date(b.fechaReserva) - new Date(a.fechaReserva))
            .slice(0, 5);

        reservasRecientes.forEach(reserva => {
            const nombreMascota = reserva.nombreMascota || 'Mascota';
            const nombreCliente = reserva.nombreCliente || 'Cliente';

            // Los servicios vienen en la lista
            const serviciosNombres = reserva.servicios && reserva.servicios.length > 0
                ? reserva.servicios.map(s => s.nombre || s.nombreServicio).join(', ')
                : 'Servicio';

            const fecha = new Date(reserva.fechaReserva);

            // Estado es string
            let estadoTexto = reserva.estado || 'Pendiente';

            // Crear timestamp combinando fecha + hora
            const horaInicio = reserva.horaInicio || '00:00:00';
            const [horas, minutos] = horaInicio.split(':');
            const timestampFecha = new Date(reserva.fechaReserva);
            timestampFecha.setHours(parseInt(horas, 10), parseInt(minutos, 10));

            actividades.push({
                tipo: 'reserva',
                titulo: `Reserva ${estadoTexto.toLowerCase()} - ${nombreMascota}`,
                desc: `${nombreCliente} · ${serviciosNombres}`,
                hora: horaInicio.substring(0, 5),
                timestamp: timestampFecha.getTime()
            });
        });
    }

    // Agregar ventas/pagos recientes
    if (ventas && ventas.length > 0) {
        const ventasRecientes = ventas
            .filter(v => {
                const fechaVenta = v.fechaVenta || v.fecha;
                return (v.estado === 1 || v.estado === 'Confirmada') &&
                       fechaVenta &&
                       esReciente(fechaVenta, true);
            })
            .sort((a, b) => {
                const fechaA = new Date(a.fechaVenta || a.fecha);
                const fechaB = new Date(b.fechaVenta || b.fecha);
                return fechaB - fechaA;
            })
            .slice(0, 5);

        ventasRecientes.forEach(venta => {
            const fecha = new Date(venta.fechaVenta || venta.fecha);
            let metodoPago = 'Efectivo';
            if (venta.metodoPago === 1 || venta.metodoPago === 'Tarjeta') metodoPago = 'Tarjeta';
            else if (venta.metodoPago === 2 || venta.metodoPago === 'Transferencia') metodoPago = 'Transferencia';

            actividades.push({
                tipo: 'pago',
                titulo: `Pago recibido - ${formatCurrency(venta.total)}`,
                desc: `Método: ${metodoPago}`,
                hora: fecha.toLocaleTimeString('es-CO', { hour: '2-digit', minute: '2-digit' }),
                timestamp: fecha.getTime()
            });
        });
    }

    // Agregar caja abierta (si se abrió hoy)
    if (cajaAbierta && cajaAbierta.fechaApertura) {
        const fechaApertura = new Date(cajaAbierta.fechaApertura);
        if (esReciente(cajaAbierta.fechaApertura, true)) {
            actividades.push({
                tipo: 'caja',
                titulo: 'Caja abierta',
                desc: `Monto inicial: ${formatCurrency(cajaAbierta.montoApertura || 0)}`,
                hora: fechaApertura.toLocaleTimeString('es-CO', { hour: '2-digit', minute: '2-digit' }),
                timestamp: fechaApertura.getTime()
            });
        }
    }

    // Agregar clientes nuevos (solo de los últimos 3 días)
    if (clientes && clientes.length > 0) {
        const clientesRecientes = clientes
            .filter(c => {
                const fecha = c.fechaRegistro || c.fechaCreacion;
                return fecha && esReciente(fecha, true);
            })
            .sort((a, b) => {
                const fechaA = new Date(a.fechaRegistro || a.fechaCreacion);
                const fechaB = new Date(b.fechaRegistro || b.fechaCreacion);
                return fechaB - fechaA;
            })
            .slice(0, 3);

        clientesRecientes.forEach(cliente => {
            const fecha = new Date(cliente.fechaRegistro || cliente.fechaCreacion);
            const hora = fecha.toLocaleTimeString('es-CO', { hour: '2-digit', minute: '2-digit' });

            actividades.push({
                tipo: 'cliente',
                titulo: `Cliente registrado - ${cliente.nombre}`,
                desc: `${cliente.telefono || 'Sin teléfono'}`,
                hora: hora,
                timestamp: fecha.getTime()
            });
        });
    }

    // Agregar mascotas nuevas (solo de los últimos 3 días)
    if (mascotas && mascotas.length > 0) {
        const mascotasRecientes = mascotas
            .filter(m => {
                const fecha = m.fechaRegistro || m.fechaCreacion;
                return fecha && esReciente(fecha, true);
            })
            .sort((a, b) => {
                const fechaA = new Date(a.fechaRegistro || a.fechaCreacion);
                const fechaB = new Date(b.fechaRegistro || b.fechaCreacion);
                return fechaB - fechaA;
            })
            .slice(0, 3);

        mascotasRecientes.forEach(mascota => {
            const cliente = clientes?.find(c => c.id === mascota.idCliente);
            const fecha = new Date(mascota.fechaRegistro || mascota.fechaCreacion);
            const hora = fecha.toLocaleTimeString('es-CO', { hour: '2-digit', minute: '2-digit' });

            actividades.push({
                tipo: 'mascota',
                titulo: `Mascota registrada - ${mascota.nombre}`,
                desc: `${cliente?.nombre || 'Cliente'} · ${mascota.raza || 'Raza'}`,
                hora: hora,
                timestamp: fecha.getTime()
            });
        });
    }

    // Ordenar todas las actividades por timestamp (más reciente primero) y tomar las primeras 8
    const actividadesOrdenadas = actividades
        .sort((a, b) => (b.timestamp || 0) - (a.timestamp || 0))
        .slice(0, 8);

    console.log(`✅ Actividades recientes generadas: ${actividadesOrdenadas.length} (filtradas de las últimas 24h-3 días)`);

    return actividadesOrdenadas;
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

    console.log('Renderizando heatmap con datos:', dashboardData.heatmap);

    if (!dashboardData.heatmap || dashboardData.heatmap.length === 0) {
        container.innerHTML = '<div style="padding: 20px; text-align: center; color: #6B7280;">No hay datos de actividad semanal</div>';
        return;
    }

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
                    ${cantidad > 0 ? cantidad : ''}
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

    console.log('Renderizando servicios ranking:', dashboardData.serviciosRanking);

    if (!dashboardData.serviciosRanking || dashboardData.serviciosRanking.length === 0) {
        container.innerHTML = '<div style="padding: 20px; text-align: center; color: #6B7280;">No hay datos de rendimiento de servicios</div>';
        return;
    }

    // El máximo de cantidad para calcular el ancho de la barra
    const maxCantidad = Math.max(...dashboardData.serviciosRanking.map(s => s.cantidad), 1);

    // Colores para cada barra (gradientes)
    const colors = [
        'linear-gradient(135deg, #7C3AED, #A78BFA)',
        'linear-gradient(135deg, #3B82F6, #60A5FA)',
        'linear-gradient(135deg, #10B981, #34D399)',
        'linear-gradient(135deg, #F59E0B, #FBBF24)',
        'linear-gradient(135deg, #EC4899, #F472B6)',
        'linear-gradient(135deg, #8B5CF6, #C4B5FD)'
    ];

    container.innerHTML = dashboardData.serviciosRanking.map((servicio, idx) => {
        const porcentajeBarra = maxCantidad > 0 ? (servicio.cantidad / maxCantidad) * 100 : 0;

        return `
            <div class="sr-item">
                <div class="sr-rank">${idx + 1}</div>
                <div class="sr-name">${servicio.nombre}</div>
                <div class="sr-bar-wrap">
                    <div class="sr-bar" style="width:${porcentajeBarra}%;background:${colors[idx % colors.length]};box-shadow:0 2px 4px rgba(0,0,0,0.1)"></div>
                </div>
                <div class="sr-ingresos">${formatCurrency(servicio.ingresos || 0)}</div>
                <div class="sr-count">${servicio.cantidad} ventas</div>
            </div>
        `;
    }).join('');
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

    // Actualizar subtítulo con el total de reservas
    const subtitleEl = document.getElementById('agendaSubtitle');
    if (subtitleEl && dashboardData.agenda) {
        const total = dashboardData.agenda.length;
        subtitleEl.textContent = `${total} reserva${total !== 1 ? 's' : ''} programada${total !== 1 ? 's' : ''}`;
    }

    // Contar por estado
    if (dashboardData.agenda && dashboardData.agenda.length > 0) {
        const confirmadas = dashboardData.agenda.filter(a => a.estado === 'confirmada').length;
        const pendientes = dashboardData.agenda.filter(a => a.estado === 'pendiente').length;
        const completadas = dashboardData.agenda.filter(a => a.estado === 'completada').length;

        const tabConfirmadas = document.getElementById('tabConfirmadas');
        const tabPendientes = document.getElementById('tabPendientes');
        const tabCompletadas = document.getElementById('tabCompletadas');

        if (tabConfirmadas) tabConfirmadas.textContent = `${confirmadas} Confirmada${confirmadas !== 1 ? 's' : ''}`;
        if (tabPendientes) tabPendientes.textContent = `${pendientes} Pendiente${pendientes !== 1 ? 's' : ''}`;
        if (tabCompletadas) tabCompletadas.textContent = `${completadas} Completada${completadas !== 1 ? 's' : ''}`;
    }

    if (!dashboardData.agenda || dashboardData.agenda.length === 0) {
        container.innerHTML = '<div style="padding: 20px; text-align: center; color: #6B7280;">No hay reservas para hoy</div>';
        return;
    }

    container.innerHTML = dashboardData.agenda.map(item => `
        <div class="ag-item">
            <div class="ag-time" data-time="${item.hora || '00:00'}"></div>
            <div class="ag-icon">
                <svg fill="none" viewBox="0 0 24 24">
                    <path d="M20.84 4.61a5.5 5.5 0 00-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 00-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 000-7.78z" />
                </svg>
            </div>
            <div class="ag-info">
                <div class="ag-client">${item.cliente || 'Cliente'}</div>
                <div class="ag-service">${item.servicio || 'Servicio'} · ${item.groomer || 'Sin asignar'}</div>
            </div>
            <div class="ag-status ${item.estado || 'pendiente'}">${
                item.estado === 'confirmada' ? '✓ Confirmada' :
                item.estado === 'en-curso' ? '● En curso' :
                item.estado === 'completada' ? '✓ Completada' : '⏱ Pendiente'
            }</div>
        </div>
    `).join('');
}

function renderEquipo() {
    const container = document.getElementById('teamRanking');
    if (!container) return;

    console.log('Renderizando equipo:', dashboardData.equipo);

    if (!dashboardData.equipo || dashboardData.equipo.length === 0) {
        container.innerHTML = '<div style="padding: 20px; text-align: center; color: #6B7280;">No hay datos de empleados</div>';
        return;
    }

    container.innerHTML = dashboardData.equipo.map(empleado => {
        const initials = getInitials(empleado.nombre);
        const reservasHoy = empleado.reservasHoy || 0;

        // Color de la barra según el número de reservas
        let barColor = '#10B981'; // Verde (poca carga)
        let barWidth = Math.min(100, (reservasHoy / 10) * 100); // Máximo 10 reservas = 100%

        if (reservasHoy >= 8) {
            barColor = '#EF4444'; // Rojo (alta carga)
        } else if (reservasHoy >= 5) {
            barColor = '#F59E0B'; // Naranja (media carga)
        } else if (reservasHoy >= 3) {
            barColor = '#3B82F6'; // Azul (normal)
        }

        let estadoClass = empleado.estado === 'disponible' ? 'disponible' :
                         empleado.estado === 'servicio' ? 'en-servicio' : 'ocupado';

        return `
            <div class="tr-card">
                <div class="tr-head">
                    <div class="tr-avatar" style="background: linear-gradient(135deg, #7C3AED, #A78BFA);">${initials}</div>
                    <div class="tr-info">
                        <div class="tr-name">${empleado.nombre}</div>
                        <div class="tr-role">${empleado.rol}</div>
                    </div>
                    <div class="tr-status ${estadoClass}">${
                        empleado.estado === 'disponible' ? 'Disponible' :
                        empleado.estado === 'servicio' ? 'Con reservas' : 'Ocupado'
                    }</div>
                </div>
                <div class="tr-meta">
                    <div class="tr-meta-item">
                        <svg width="13" height="13" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5"><rect x="3" y="4" width="18" height="18" rx="2"/><path d="M16 2v4M8 2v4M3 10h18"/></svg>
                        ${reservasHoy} reservas
                    </div>
                    <div class="tr-earn">${formatCurrency(empleado.ingresos)}</div>
                </div>
                <div class="tr-bars">
                    <div class="tr-bar-row">
                        <span class="tr-bar-lbl">Ocupación</span>
                        <div class="tr-bar-track">
                            <div class="tr-bar-fill" style="width: ${barWidth}%; background: ${barColor};"></div>
                        </div>
                        <span class="tr-bar-pct">${Math.round(barWidth)}%</span>
                    </div>
                </div>
            </div>
        `;
    }).join('');
}

function renderActividades() {
    const container = document.getElementById('activityList');
    if (!container) return;

    console.log('Renderizando actividades:', dashboardData.actividades);

    if (!dashboardData.actividades || dashboardData.actividades.length === 0) {
        container.innerHTML = '<div style="padding: 20px; text-align: center; color: #6B7280;">No hay actividades recientes</div>';
        return;
    }

    const iconos = {
        reserva: '📅',
        completado: '✅',
        pago: '💰',
        mascota: '🐾',
        caja: '💵',
        cliente: '👤',
        empleado: '👨‍💼',
        servicio: '⚙️',
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

// ── FUNCIONES DE ACTUALIZACIÓN ─────────────────────────────────────

function actualizarDatos() {
    console.log('🔄 Actualización manual solicitada');
    const refreshBtn = document.getElementById('refreshBtn');

    if (refreshBtn) {
        refreshBtn.classList.add('rotating');
        refreshBtn.disabled = true;
    }

    cargarDashboard(true).finally(() => {
        if (refreshBtn) {
            setTimeout(() => {
                refreshBtn.classList.remove('rotating');
                refreshBtn.disabled = false;
            }, 500);
        }
    });
}

// ── SISTEMA DE NOTIFICACIONES ──────────────────────────────────────

let ultimasCantidadNotificaciones = 0;

// Reproducir sonido de notificación usando Web Audio API
function reproducirSonidoNotificacion() {
    try {
        const audioContext = new (window.AudioContext || window.webkitAudioContext)();

        // Crear dos tonos para un sonido de notificación agradable
        const playTone = (frequency, startTime, duration) => {
            const oscillator = audioContext.createOscillator();
            const gainNode = audioContext.createGain();

            oscillator.frequency.value = frequency;
            oscillator.type = 'sine';

            gainNode.gain.setValueAtTime(0.3, startTime);
            gainNode.gain.exponentialRampToValueAtTime(0.01, startTime + duration);

            oscillator.connect(gainNode);
            gainNode.connect(audioContext.destination);

            oscillator.start(startTime);
            oscillator.stop(startTime + duration);
        };

        // Dos tonos: E6 y C6
        playTone(1318.51, audioContext.currentTime, 0.1);
        playTone(1046.50, audioContext.currentTime + 0.1, 0.15);

        console.log('🔊 Sonido de notificación reproducido');
    } catch (error) {
        console.error('❌ Error al reproducir sonido:', error);
    }
}

async function cargarNotificaciones() {
    try {
        const token = localStorage.getItem('token');
        if (!token) {
            console.log('ℹ️ No hay token, saltando carga de notificaciones');
            return [];
        }

        const response = await fetch(`${API_BASE}/Notificacion/no-leidas`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (!response.ok) {
            console.error('❌ Error al cargar notificaciones:', response.status);
            return [];
        }

        const notificaciones = await response.json();
        const noLeidas = notificaciones.length;

        console.log('🔔 Notificaciones cargadas:', notificaciones.length, 'no leídas');

        // Reproducir sonido si hay nuevas notificaciones
        if (noLeidas > ultimasCantidadNotificaciones && ultimasCantidadNotificaciones > 0) {
            reproducirSonidoNotificacion();
        }
        ultimasCantidadNotificaciones = noLeidas;

        // Actualizar badge
        const badge = document.getElementById('notifBadge');
        if (badge) {
            if (noLeidas > 0) {
                badge.style.display = 'block';
                badge.textContent = noLeidas > 9 ? '9+' : noLeidas;
                console.log('✅ Badge actualizado:', noLeidas);
            } else {
                badge.style.display = 'none';
                console.log('ℹ️ Badge oculto (no hay notificaciones no leídas)');
            }
        } else {
            console.warn('⚠️ Elemento notifBadge no encontrado');
        }

        return notificaciones;
    } catch (error) {
        console.error('❌ Error al cargar notificaciones:', error);
        return [];
    }
}

async function renderNotificaciones() {
    console.log('📋 Renderizando notificaciones...');
    const notificaciones = await cargarNotificaciones();
    const container = document.getElementById('notifList');

    if (!container) {
        console.error('❌ Elemento notifList no encontrado');
        return;
    }

    console.log('📊 Total de notificaciones a renderizar:', notificaciones.length);

    if (notificaciones.length === 0) {
        container.innerHTML = '<div style="padding: 40px 20px; text-align: center; color: var(--muted);">No hay notificaciones</div>';
        console.log('ℹ️ No hay notificaciones para mostrar');
        return;
    }

    container.innerHTML = notificaciones.map(notif => {
        const fecha = new Date(notif.fechaCreacion);
        const ahora = new Date();
        const diffMinutos = Math.floor((ahora - fecha) / 60000);

        let tiempoTexto;
        if (diffMinutos < 1) {
            tiempoTexto = 'Ahora';
        } else if (diffMinutos < 60) {
            tiempoTexto = `Hace ${diffMinutos} min`;
        } else if (diffMinutos < 1440) {
            const horas = Math.floor(diffMinutos / 60);
            tiempoTexto = `Hace ${horas} hora${horas > 1 ? 's' : ''}`;
        } else {
            const dias = Math.floor(diffMinutos / 1440);
            tiempoTexto = `Hace ${dias} día${dias > 1 ? 's' : ''}`;
        }

        return `
            <div class="notif-item unread" onclick="marcarNotificacionLeida(${notif.id})">
                <div class="notif-icon">${notif.icono || '📋'}</div>
                <div class="notif-content">
                    <div class="notif-title">${notif.titulo}</div>
                    <div class="notif-message">${notif.mensaje}</div>
                    <div class="notif-time">${tiempoTexto}</div>
                </div>
            </div>
        `;
    }).join('');
}

function toggleNotificaciones() {
    console.log('🔔 Toggle notificaciones...');
    const dropdown = document.getElementById('notifDropdown');

    if (!dropdown) {
        console.error('❌ Elemento notifDropdown no encontrado');
        return;
    }

    const isVisible = dropdown.style.display === 'block';
    console.log('📊 Dropdown visible:', isVisible);

    if (isVisible) {
        dropdown.style.display = 'none';
        console.log('✅ Dropdown ocultado');
    } else {
        renderNotificaciones();
        dropdown.style.display = 'block';
        console.log('✅ Dropdown mostrado');
    }
}

// Exponer funciones globalmente para que puedan ser llamadas desde HTML
window.toggleNotificaciones = toggleNotificaciones;

async function marcarNotificacionLeida(id) {
    try {
        const token = localStorage.getItem('token');
        if (!token) return;

        const response = await fetch(`${API_BASE}/Notificacion/${id}/marcar-leida`, {
            method: 'PUT',
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (response.ok) {
            const notif = await response.json();
            await renderNotificaciones();

            // Si la notificación tiene reserva, navegar a Reservas
            if (notif.idReserva) {
                window.location.href = '/Admin/Reservas.html';
            }
        }
    } catch (error) {
        console.error('Error al marcar notificación como leída:', error);
    }
}

// Exponer función globalmente
window.marcarNotificacionLeida = marcarNotificacionLeida;

async function marcarTodasLeidas() {
    try {
        const token = localStorage.getItem('token');
        if (!token) return;

        const response = await fetch(`${API_BASE}/Notificacion/marcar-todas-leidas`, {
            method: 'PUT',
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (response.ok) {
            await renderNotificaciones();
        }
    } catch (error) {
        console.error('Error al marcar todas como leídas:', error);
    }
}

// Exponer función globalmente
window.marcarTodasLeidas = marcarTodasLeidas;

// Cerrar dropdown al hacer clic fuera
document.addEventListener('click', function(e) {
    const notifWrap = document.querySelector('.notifications-wrap');
    const notifDropdown = document.getElementById('notifDropdown');

    if (notifWrap && !notifWrap.contains(e.target) && notifDropdown) {
        notifDropdown.style.display = 'none';
    }
});

// ── INICIALIZACIÓN ──────────────────────────────────────────────────

document.addEventListener('DOMContentLoaded', () => {
    // Cargar dashboard (usará caché si está disponible)
    cargarDashboard();

    // Cargar notificaciones
    cargarNotificaciones();

    // Actualizar cada 2 minutos (pero usará caché si es válida)
    setInterval(() => {
        console.log('🔄 Actualización automática programada');
        cargarDashboard(false);
        cargarNotificaciones(); // También actualizar notificaciones
    }, 2 * 60 * 1000);

    // Detectar cuando la pestaña se vuelve visible
    document.addEventListener('visibilitychange', () => {
        if (!document.hidden) {
            console.log('👁️ Pestaña visible, verificando datos...');
            // Solo recargar si la caché está vencida
            if (!isCacheValid()) {
                cargarDashboard(false);
            }
            cargarNotificaciones(); // Actualizar notificaciones al volver
        }
    });
});
