// ═══════════════════════════════════════════════════════
// HISTORIAL CLÍNICO - JavaScript
// ═══════════════════════════════════════════════════════

const API = '';
let mascotas = [];
let historiales = [];
let reservas = [];
let clientes = [];
let empleados = [];
let mascotaSeleccionada = null;
let filtroActual = 'all';

// ═══════════════════════════════════════════════════════
// PROTECCIÓN DE RUTA Y AUTENTICACIÓN
// ═══════════════════════════════════════════════════════

function check() {
    if (!localStorage.getItem('token'))
        window.location.replace('/Admin/Login.html');
}
check();
window.addEventListener('pageshow', check);

// ═══════════════════════════════════════════════════════
// CONFIGURACIÓN INICIAL
// ═══════════════════════════════════════════════════════

const DIAS = ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado'];
const MESES = ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
    'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'];

const hoy = new Date();
document.getElementById('fechaHoy').textContent =
    DIAS[hoy.getDay()] + ', ' + hoy.getDate() + ' de ' +
    MESES[hoy.getMonth()] + ' de ' + hoy.getFullYear();

// JWT
let uName = 'Admin', uEmail = '', uRole = 'Administrador', uIni = 'AD';
try {
    var tok = localStorage.getItem('token');
    if (tok) {
        var p = JSON.parse(atob(tok.split('.')[1]));
        uName = p['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || uName;
        uEmail = p['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || uEmail;
        uRole = p['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || uRole;
        uIni = uName.trim().split(' ').map(x => x[0]).join('').substring(0, 2).toUpperCase();
    }
} catch (e) { }

document.getElementById('tbAva').textContent = uIni;
document.getElementById('tbName').textContent = uName;
document.getElementById('tbRole').textContent = uRole;
document.getElementById('ddName').textContent = uName;
document.getElementById('ddEmail').textContent = uEmail;

// ═══════════════════════════════════════════════════════
// FUNCIONES UI GLOBALES
// ═══════════════════════════════════════════════════════

function toggleSidebar() { document.getElementById('sidebar').classList.toggle('collapsed'); }

function toggleDrop() {
    var d = document.getElementById('tbDrop');
    var p = document.getElementById('tbProfile');
    var o = d.classList.toggle('open');
    p.classList.toggle('open', o);
}

document.addEventListener('click', function (e) {
    var w = document.querySelector('.tb-profile-wrap');
    if (w && !w.contains(e.target)) {
        document.getElementById('tbDrop').classList.remove('open');
        document.getElementById('tbProfile').classList.remove('open');
    }
});

var TK = 'venus_theme';
var dark = localStorage.getItem(TK) === 'dark';
function applyTheme(d) {
    document.documentElement.setAttribute('data-theme', d ? 'dark' : 'light');
    document.getElementById('iconSun').style.display = d ? 'none' : 'block';
    document.getElementById('iconMoon').style.display = d ? 'block' : 'none';
    localStorage.setItem(TK, d ? 'dark' : 'light');
}
function toggleTheme() { dark = !dark; applyTheme(dark); }
applyTheme(dark);

function logout() { localStorage.removeItem('token'); window.location.replace('/Admin/Login.html'); }

function authHeaders() {
    return { 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + localStorage.getItem('token') };
}

function toast(msg, tipo) {
    tipo = tipo || 'success';
    var el = document.getElementById('toast');
    el.textContent = msg;
    el.className = 'toast ' + tipo;
    el.style.display = 'block';
    clearTimeout(el._t);
    el._t = setTimeout(() => { el.style.display = 'none'; }, 3200);
}

// ═══════════════════════════════════════════════════════
// CARGA DE DATOS
// ═══════════════════════════════════════════════════════

async function cargarDatos() {
    try {
        const [resMascotas, resClientes, resEmpleados] = await Promise.all([
            fetch(API + '/api/Mascota', { headers: authHeaders() }),
            fetch(API + '/api/Cliente', { headers: authHeaders() }),
            fetch(API + '/api/Empleado', { headers: authHeaders() })
        ]);

        if (resMascotas.status === 401) { window.location.replace('/Admin/Login.html'); return; }
        if (!resMascotas.ok) throw new Error('Error al cargar mascotas');

        mascotas = await resMascotas.json();
        clientes = resClientes.ok ? await resClientes.json() : [];
        empleados = resEmpleados.ok ? await resEmpleados.json() : [];

        // Enriquecer mascotas con información del cliente
        mascotas.forEach(m => {
            const cliente = clientes.find(c => c.id === m.idCliente);
            if (cliente) {
                m.nombreCliente = cliente.nombre;
                m.telefonoCliente = cliente.telefono;
            }
        });

        renderMascotasList();

    } catch (err) {
        console.error(err);
        document.getElementById('mascotasList').innerHTML =
            '<div class="empty-state-list">Error al cargar los datos: ' + err.message + '</div>';
    }
}

// ═══════════════════════════════════════════════════════
// RENDERIZADO DE LISTA DE MASCOTAS
// ═══════════════════════════════════════════════════════

function renderMascotasList() {
    const container = document.getElementById('mascotasList');

    let mascotasFiltradas = mascotas;

    // Aplicar filtro por tamaño
    if (filtroActual !== 'all') {
        mascotasFiltradas = mascotasFiltradas.filter(m => {
            // Normalizar comparación para manejar "Pequeno" y "Pequeño"
            const tamanioNormalizado = m.tamaño.toLowerCase().normalize("NFD").replace(/[\u0300-\u036f]/g, "");
            const filtroNormalizado = filtroActual.toLowerCase().normalize("NFD").replace(/[\u0300-\u036f]/g, "");
            return tamanioNormalizado === filtroNormalizado;
        });
    }

    // Aplicar búsqueda
    const searchTerm = document.getElementById('searchMascota').value.toLowerCase();
    if (searchTerm) {
        mascotasFiltradas = mascotasFiltradas.filter(m =>
            m.nombre.toLowerCase().includes(searchTerm) ||
            (m.nombreCliente && m.nombreCliente.toLowerCase().includes(searchTerm)) ||
            m.raza.toLowerCase().includes(searchTerm)
        );
    }

    if (mascotasFiltradas.length === 0) {
        container.innerHTML = '<div class="empty-state-list">No se encontraron mascotas</div>';
        return;
    }

    container.innerHTML = mascotasFiltradas.map(m => {
        const iniciales = m.nombre.substring(0, 2).toUpperCase();
        const isSelected = mascotaSeleccionada && mascotaSeleccionada.id === m.id;

        return `
            <div class="mascota-card ${isSelected ? 'selected' : ''}" onclick="seleccionarMascota(${m.id})">
                <div class="mascota-card-header">
                    <div class="mascota-photo">
                        ${m.imagen ? `<img src="${m.imagen}" alt="${m.nombre}">` : iniciales}
                    </div>
                    <div class="mascota-info-card">
                        <div class="mascota-name">${m.nombre}</div>
                        <div class="mascota-owner">
                            <svg fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                                <path d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/>
                            </svg>
                            ${m.nombreCliente || 'Sin dueño'}
                        </div>
                    </div>
                </div>
                <div class="mascota-meta">
                    <div class="meta-item">
                        <svg fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                            <path d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z"/>
                        </svg>
                        ${m.raza}
                    </div>
                    <div class="meta-item">
                        <svg fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                            <circle cx="12" cy="12" r="10"/>
                            <path d="M12 6v6l4 2"/>
                        </svg>
                        ${m.edad} ${m.edad === 1 ? 'año' : 'años'}
                    </div>
                    <div class="meta-item">
                        <svg fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                            <path d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"/>
                        </svg>
                        ${m.tamaño}
                    </div>
                </div>
            </div>
        `;
    }).join('');
}

// ═══════════════════════════════════════════════════════
// SELECCIÓN DE MASCOTA
// ═══════════════════════════════════════════════════════

async function seleccionarMascota(id) {
    mascotaSeleccionada = mascotas.find(m => m.id === id);
    if (!mascotaSeleccionada) return;

    // Actualizar UI de lista
    renderMascotasList();

    // Cargar historial y reservas
    await cargarDatosMascota(id);

    // Renderizar detalle
    renderDetalleMascota();
}

async function cargarDatosMascota(idMascota) {
    try {
        const [resHistorial, resReservas] = await Promise.all([
            fetch(API + `/api/Historial/mascota/${idMascota}`, { headers: authHeaders() }),
            fetch(API + `/api/Reserva/mascota/${idMascota}`, { headers: authHeaders() })
        ]);

        historiales = resHistorial.ok ? await resHistorial.json() : [];
        reservas = resReservas.ok ? await resReservas.json() : [];

    } catch (err) {
        console.error('Error al cargar datos de mascota:', err);
        historiales = [];
        reservas = [];
    }
}

// ═══════════════════════════════════════════════════════
// RENDER DEL DETALLE DE MASCOTA
// ═══════════════════════════════════════════════════════

function renderDetalleMascota() {
    const m = mascotaSeleccionada;
    const iniciales = m.nombre.substring(0, 2).toUpperCase();

    // Calcular métricas
    const numVisitas = reservas.length;
    const totalGastado = reservas.reduce((sum, r) => sum + (r.precioTotal || 0), 0);

    // Última visita: fecha de la reserva más reciente
    let ultimaVisita = 'Sin visitas';
    if (reservas.length > 0) {
        // Ordenar reservas por fecha descendente y tomar la primera
        const reservasOrdenadas = [...reservas].sort((a, b) =>
            new Date(b.fechaReserva) - new Date(a.fechaReserva)
        );
        ultimaVisita = formatFecha(reservasOrdenadas[0].fechaReserva);
    }

    const fechaNacimiento = m.fechaNacimiento
        ? formatFecha(m.fechaNacimiento)
        : 'No especificada';

    const html = `
        <div class="detail-header">
            <div class="detail-photo">
                ${m.imagen ? `<img src="${m.imagen}" alt="${m.nombre}">` : iniciales}
            </div>
            <div class="detail-info">
                <h1 class="detail-title">${m.nombre}</h1>
                <div class="detail-tags">
                    <span class="tag">
                        <svg fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                            <path d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"/>
                        </svg>
                        ${m.tamaño}
                    </span>
                    <span class="tag">${m.raza}</span>
                    <span class="tag">${m.edad} ${m.edad === 1 ? 'año' : 'años'}</span>
                    <span class="tag">
                        <svg fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                            <path d="M7 21a4 4 0 01-4-4V5a2 2 0 012-2h4a2 2 0 012 2v12a4 4 0 01-4 4zm0 0h12a2 2 0 002-2v-4a2 2 0 00-2-2h-2.343M11 7.343l1.657-1.657a2 2 0 012.828 0l2.829 2.829a2 2 0 010 2.828l-8.486 8.485M7 17h.01"/>
                        </svg>
                        ${m.tipoPelaje}
                    </span>
                </div>
                <div class="detail-owner-info">
                    <svg fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                        <path d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/>
                    </svg>
                    <div>
                        <div class="owner-text">Dueño</div>
                        <div class="owner-name">${m.nombreCliente || 'No asignado'}</div>
                    </div>
                </div>
            </div>
        </div>

        <div class="detail-metrics">
            <div class="metric-card">
                <div class="metric-value">${numVisitas}</div>
                <div class="metric-label">Visitas</div>
            </div>
            <div class="metric-card">
                <div class="metric-value">$${totalGastado.toLocaleString('es-CO')}</div>
                <div class="metric-label">Total Gastado</div>
            </div>
            <div class="metric-card">
                <div class="metric-value">${ultimaVisita}</div>
                <div class="metric-label">Última Visita</div>
            </div>
            <div class="metric-card">
                <div class="metric-value">${fechaNacimiento}</div>
                <div class="metric-label">Nacimiento</div>
            </div>
        </div>

        <div class="detail-tabs">
            <button class="detail-tab active" onclick="cambiarTab('historial')">Historial</button>
            <button class="detail-tab" onclick="cambiarTab('reservas')">Reservas</button>
        </div>

        <div class="tab-content active" id="tab-historial">
            ${renderTabHistorial()}
        </div>

        <div class="tab-content" id="tab-reservas">
            ${renderTabReservas()}
        </div>
    `;

    document.getElementById('mascotaDetailContent').innerHTML = html;
}

// ═══════════════════════════════════════════════════════
// RENDER TAB HISTORIAL
// ═══════════════════════════════════════════════════════

function renderTabHistorial() {
    const btnAgregar = `
        <div style="margin-bottom: 20px; display: flex; justify-content: flex-end;">
            <button class="btn-add-historial" onclick="abrirModalHistorial()">
                <svg fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                    <path d="M12 4v16m8-8H4"/>
                </svg>
                Agregar Registro
            </button>
        </div>
    `;

    if (historiales.length === 0) {
        return btnAgregar + '<div class="empty-state-list">No hay registros en el historial clínico</div>';
    }

    return btnAgregar + `
        <div class="historial-timeline">
            ${historiales.map(h => `
                <div class="historial-item">
                    <div class="historial-item-header">
                        <div class="historial-fecha-codigo">
                            <div class="historial-fecha">${formatFechaCompleta(h.fechaAtencion)}</div>
                            <div class="historial-codigo">Código: HC-${h.id.toString().padStart(6, '0')}</div>
                        </div>
                        <span class="historial-estado-badge ${(h.estado || 'completada').toLowerCase()}">
                            Completada
                        </span>
                    </div>
                    <div class="historial-item-body">
                        <div class="historial-info-group">
                            <div class="info-row">
                                <svg fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                                    <circle cx="12" cy="12" r="10"/>
                                    <path d="M12 6v6l4 2"/>
                                </svg>
                                <span class="info-label">Horario:</span>
                                <span class="info-value">${formatHora(h.fechaAtencion)}</span>
                            </div>
                            <div class="info-row">
                                <svg fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                                    <path d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/>
                                </svg>
                                <span class="info-label">Empleado:</span>
                                <span class="info-value">${h.nombreEmpleado || 'No especificado'}</span>
                            </div>
                        </div>
                        <div class="historial-info-group">
                            <div class="info-row">
                                <svg fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                                    <path d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"/>
                                </svg>
                                <span class="info-label">Código Reserva:</span>
                                <span class="info-value">${h.codigoReserva || 'N/A'}</span>
                            </div>
                        </div>
                    </div>
                    ${h.recomendaciones ? `
                    <div class="historial-recomendaciones">
                        <div class="recomendaciones-header">
                            <svg fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                                <path d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/>
                            </svg>
                            Recomendaciones y Observaciones
                        </div>
                        <div class="recomendaciones-text">${h.recomendaciones}</div>
                    </div>
                    ` : ''}
                </div>
            `).join('')}
        </div>
    `;
}

// ═══════════════════════════════════════════════════════
// RENDER TAB RESERVAS
// ═══════════════════════════════════════════════════════

function renderTabReservas() {
    if (reservas.length === 0) {
        return '<div class="empty-state-list">No hay reservas registradas para esta mascota</div>';
    }

    return `
        <div class="reservas-list">
            ${reservas.map(r => {
                const nombreEmpleado = r.empleado?.nombre || r.nombreEmpleado || 'Sin asignar';
                const observaciones = r.observaciones || r.notas || 'Sin observaciones registradas para esta reserva.';
                const estadoClass = (r.estado || 'Pendiente').toLowerCase().replace(' ', '-');

                return `
                <div class="reserva-card-nueva">
                    <div class="reserva-header-nueva">
                        <div class="reserva-info-principal">
                            <div class="reserva-fecha-codigo-nueva">
                                <div class="reserva-fecha-grande">${formatFecha(r.fechaReserva)}</div>
                                <div class="reserva-codigo-nuevo">${r.codigoReserva || 'RES-' + r.id}</div>
                            </div>
                            <div class="reserva-detalles-row">
                                <div class="detalle-item">
                                    <svg fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                                        <circle cx="12" cy="12" r="10"/>
                                        <path d="M12 6v6l4 2"/>
                                    </svg>
                                    <span>${formatHoraReserva(r.horaInicio)} - ${formatHoraReserva(r.horaFin)}</span>
                                </div>
                                <div class="detalle-item">
                                    <svg fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                                        <path d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/>
                                    </svg>
                                    <span>${r.duracionMinutos || 60} minutos</span>
                                </div>
                                <div class="detalle-item">
                                    <svg fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                                        <path d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/>
                                    </svg>
                                    <span>${nombreEmpleado}</span>
                                </div>
                            </div>
                        </div>
                        <div class="reserva-total-estado">
                            <div class="reserva-total-nueva">$${(r.precioTotal || 0).toLocaleString('es-CO')}</div>
                            <div class="reserva-estado-badge ${estadoClass}">${r.estado || 'Pendiente'}</div>
                        </div>
                    </div>

                    <div class="reserva-observaciones">
                        <div class="observaciones-icon">
                            <svg fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                                <path d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/>
                            </svg>
                        </div>
                        <div class="observaciones-texto">${observaciones}</div>
                    </div>

                    ${r.reservaServicios && r.reservaServicios.length > 0 ? `
                    <div class="reserva-servicios-tags">
                        <div class="servicios-tags-label">Servicios realizados:</div>
                        <div class="servicios-tags-container">
                            ${r.reservaServicios.map(s => `
                                <div class="servicio-tag">
                                    <span class="servicio-tag-nombre">${s.nombreServicio || 'Servicio'}</span>
                                    <span class="servicio-tag-precio">$${(s.precioUnitario || 0).toLocaleString('es-CO')}</span>
                                </div>
                            `).join('')}
                        </div>
                    </div>
                    ` : ''}
                </div>
            `}).join('')}
        </div>
    `;
}

// ═══════════════════════════════════════════════════════
// CAMBIO DE TABS
// ═══════════════════════════════════════════════════════

function cambiarTab(tab, event) {
    // Validar que event exista
    if (!event || !event.target) return;

    // Actualizar botones
    document.querySelectorAll('.detail-tab').forEach(t => t.classList.remove('active'));
    event.target.classList.add('active');

    // Actualizar contenido
    document.querySelectorAll('.tab-content').forEach(t => t.classList.remove('active'));
    document.getElementById('tab-' + tab).classList.add('active');
}

// ═══════════════════════════════════════════════════════
// FILTROS Y BÚSQUEDA
// ═══════════════════════════════════════════════════════

function filtrarPorTamanio(tamanio) {
    filtroActual = tamanio;

    // Actualizar botones de filtro
    document.querySelectorAll('.filter-tab').forEach(tab => {
        tab.classList.remove('active');
        if (tab.dataset.filter === tamanio) {
            tab.classList.add('active');
        }
    });

    renderMascotasList();
}

// Búsqueda en tiempo real
document.addEventListener('DOMContentLoaded', () => {
    const searchInput = document.getElementById('searchMascota');
    if (searchInput) {
        searchInput.addEventListener('input', () => {
            renderMascotasList();
        });
    }
});

// ═══════════════════════════════════════════════════════
// UTILIDADES DE FORMATO
// ═══════════════════════════════════════════════════════

function formatFecha(fecha) {
    if (!fecha) return '-';
    const f = new Date(fecha);
    return f.toLocaleDateString('es-CO', { day: '2-digit', month: 'short', year: 'numeric' });
}

function formatFechaCompleta(fecha) {
    if (!fecha) return '-';
    const f = new Date(fecha);
    return f.toLocaleDateString('es-CO', {
        weekday: 'long',
        day: 'numeric',
        month: 'long',
        year: 'numeric'
    });
}

function formatHora(fecha) {
    if (!fecha) return '-';
    const f = new Date(fecha);
    return f.toLocaleTimeString('es-CO', { hour: '2-digit', minute: '2-digit', hour12: false });
}

function formatHoraReserva(hora) {
    if (!hora) return '-';
    return hora.substring(0, 5);
}

// ═══════════════════════════════════════════════════════
// FUNCIONES PARA CREAR HISTORIAL
// ═══════════════════════════════════════════════════════

function abrirModalHistorial() {
    if (!mascotaSeleccionada) {
        toast('Primero selecciona una mascota', 'error');
        return;
    }

    // Limpiar formulario
    document.getElementById('formHistorial').reset();
    document.getElementById('historialIdMascota').value = mascotaSeleccionada.id;

    // Cargar empleados en el select
    const selectEmpleado = document.getElementById('historialIdEmpleado');
    selectEmpleado.innerHTML = '<option value="">Seleccione un empleado</option>' +
        empleados.map(e => `<option value="${e.id}">${e.nombre} - ${e.cargo}</option>`).join('');

    // Cargar reservas de la mascota en el select
    const selectReserva = document.getElementById('historialIdReserva');
    selectReserva.innerHTML = '<option value="">Sin reserva asociada</option>' +
        reservas.map(r => `<option value="${r.id}">${r.codigoReserva} - ${formatFecha(r.fechaReserva)}</option>`).join('');

    // Mostrar modal
    document.getElementById('modalHistorial').style.display = 'flex';
}

function cerrarModalHistorial() {
    document.getElementById('modalHistorial').style.display = 'none';
}

async function guardarHistorial(event) {
    event.preventDefault();

    const idMascota = parseInt(document.getElementById('historialIdMascota').value);
    const idEmpleado = parseInt(document.getElementById('historialIdEmpleado').value);
    const idReservaVal = document.getElementById('historialIdReserva').value;
    const idReserva = idReservaVal ? parseInt(idReservaVal) : null;
    const recomendaciones = document.getElementById('historialRecomendaciones').value.trim();

    if (!idMascota || !idEmpleado || !recomendaciones) {
        toast('Complete todos los campos requeridos', 'error');
        return;
    }

    try {
        const dto = {
            IdMascota: idMascota,
            IdEmpleado: idEmpleado,
            IdReserva: idReserva,
            Recomendaciones: recomendaciones
        };

        const response = await fetch(API + '/api/Historial', {
            method: 'POST',
            headers: authHeaders(),
            body: JSON.stringify(dto)
        });

        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.message || 'Error al crear el historial');
        }

        toast('Registro de historial creado exitosamente', 'success');
        cerrarModalHistorial();

        // Recargar datos de la mascota
        await cargarDatosMascota(idMascota);
        renderDetalleMascota();

    } catch (err) {
        console.error('Error al guardar historial:', err);
        toast(err.message || 'Error al guardar el historial', 'error');
    }
}

// Cerrar modal al hacer clic fuera
document.addEventListener('click', function(e) {
    const modal = document.getElementById('modalHistorial');
    if (modal && e.target === modal) {
        cerrarModalHistorial();
    }
});

// ═══════════════════════════════════════════════════════
// INICIALIZACIÓN
// ═══════════════════════════════════════════════════════

cargarDatos();
