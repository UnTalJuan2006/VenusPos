// ═══════════════════════════════════════════════════════════════════════════
// VENUSPOS - RESERVA API
// API wrapper functions for reservation endpoints
// ═══════════════════════════════════════════════════════════════════════════

const API_BASE = '/api';

// ───────────────────────────────────────────────────────────────────────────
// Helper Functions
// ───────────────────────────────────────────────────────────────────────────

async function fetchAPI(url, options = {}) {
    try {
        const response = await fetch(`${API_BASE}${url}`, {
            headers: {
                'Content-Type': 'application/json',
                ...options.headers
            },
            ...options
        });

        if (!response.ok) {
            const error = await response.json().catch(() => ({ message: 'Error en la solicitud' }));
            throw new Error(error.message || `HTTP ${response.status}`);
        }

        return await response.json();
    } catch (error) {
        console.error('API Error:', error);
        throw error;
    }
}

// ───────────────────────────────────────────────────────────────────────────
// Cliente API
// ───────────────────────────────────────────────────────────────────────────

const ClienteAPI = {
    // Login cliente by email
    async login(email) {
        return await fetchAPI('/Cliente/login', {
            method: 'POST',
            body: JSON.stringify({ email })
        });
    },

    // Register new cliente
    async registrar(data) {
        return await fetchAPI('/Cliente', {
            method: 'POST',
            body: JSON.stringify(data)
        });
    },

    // Get cliente by ID
    async obtenerPorId(id) {
        return await fetchAPI(`/Cliente/${id}`);
    }
};

// ───────────────────────────────────────────────────────────────────────────
// Mascota API
// ───────────────────────────────────────────────────────────────────────────

const MascotaAPI = {
    // Get mascotas by cliente ID
    async obtenerPorCliente(idCliente) {
        return await fetchAPI(`/Mascota/cliente/${idCliente}`);
    },

    // Get mascota by ID
    async obtenerPorId(id) {
        return await fetchAPI(`/Mascota/${id}`);
    },

    // Create new mascota
    async crear(data) {
        return await fetchAPI('/Mascota', {
            method: 'POST',
            body: JSON.stringify(data)
        });
    },

    // Upload mascota image
    async uploadImagen(file) {
        const formData = new FormData();
        formData.append('imagen', file);

        const response = await fetch(`${API_BASE}/Mascota/upload-imagen`, {
            method: 'POST',
            body: formData
        });

        if (!response.ok) {
            const error = await response.json().catch(() => ({ message: 'Error al subir imagen' }));
            throw new Error(error.message);
        }

        return await response.json();
    }
};

// ───────────────────────────────────────────────────────────────────────────
// Servicio API
// ───────────────────────────────────────────────────────────────────────────

const ServicioAPI = {
    // Get all active servicios
    async obtenerTodos() {
        return await fetchAPI('/Servicio');
    },

    // Get servicio by ID
    async obtenerPorId(id) {
        return await fetchAPI(`/Servicio/${id}`);
    }
};

// ───────────────────────────────────────────────────────────────────────────
// Empleado API
// ───────────────────────────────────────────────────────────────────────────

const EmpleadoAPI = {
    // Get all empleados
    async obtenerTodos() {
        return await fetchAPI('/Empleado');
    },

    // Get empleado by ID
    async obtenerPorId(id) {
        return await fetchAPI(`/Empleado/${id}`);
    }
};

// ───────────────────────────────────────────────────────────────────────────
// Reserva API
// ───────────────────────────────────────────────────────────────────────────

const ReservaAPI = {
    // Calculate price for reservation
    async calcularPrecio(idMascota, idsServicios) {
        return await fetchAPI('/Reserva/calcular-precio', {
            method: 'POST',
            body: JSON.stringify({
                idMascota,
                idsServicios
            })
        });
    },

    // Get available time slots
    async obtenerDisponibilidad(fecha, duracionMinutos, idEmpleado = null, tamañoMascota = null) {
        let url = `/Reserva/disponibilidad?fecha=${fecha}&duracionMinutos=${duracionMinutos}`;
        if (idEmpleado) {
            url += `&idEmpleado=${idEmpleado}`;
        }
        if (tamañoMascota) {
            url += `&tamañoMascota=${tamañoMascota}`;
        }
        return await fetchAPI(url);
    },

    // Get available schedules (alias)
    async obtenerHorariosDisponibles(fecha, duracionMinutos, idEmpleado = null, tamañoMascota = null) {
        let url = `/Reserva/disponibilidad?fecha=${fecha}&duracionMinutos=${duracionMinutos}`;
        if (idEmpleado) {
            url += `&idEmpleado=${idEmpleado}`;
        }
        if (tamañoMascota) {
            url += `&tamañoMascota=${tamañoMascota}`;
        }
        return await fetchAPI(url);
    },

    // Create new reservation
    async crear(data) {
        return await fetchAPI('/Reserva', {
            method: 'POST',
            body: JSON.stringify({
                idCliente: data.idCliente,
                idMascota: data.idMascota,
                idEmpleado: data.idEmpleado,
                fechaReserva: data.fechaReserva,
                horaInicio: data.horaInicio,
                idsServicios: data.idsServicios,
                detalles: data.detalles || ''
            })
        });
    },

    // Confirm reservation (generate code and send email)
    async confirmar(idReserva) {
        return await fetchAPI(`/Reserva/${idReserva}/confirmar`, {
            method: 'POST'
        });
    },

    // Get reservation by ID
    async obtenerPorId(id) {
        return await fetchAPI(`/Reserva/${id}`);
    },

    // Get reservation by code
    async obtenerPorCodigo(codigo) {
        return await fetchAPI(`/Reserva/codigo/${codigo}`);
    },

    // Get reservations by cliente
    async obtenerPorCliente(idCliente) {
        return await fetchAPI(`/Reserva/cliente/${idCliente}`);
    },

    // Cancel reservation
    async cancelar(id) {
        return await fetchAPI(`/Reserva/${id}`, {
            method: 'DELETE'
        });
    }
};

// ───────────────────────────────────────────────────────────────────────────
// ConfiguracionPrecio API (Admin)
// ───────────────────────────────────────────────────────────────────────────

const ConfiguracionPrecioAPI = {
    // Get all price configurations
    async obtenerTodos() {
        return await fetchAPI('/ConfiguracionPrecio');
    },

    // Update price configuration
    async actualizar(id, valor) {
        return await fetchAPI(`/ConfiguracionPrecio/${id}`, {
            method: 'PUT',
            body: JSON.stringify({ valor })
        });
    }
};

// Debug log
console.log('reserva-api.js loaded');
console.log('ReservaAPI methods:', Object.keys(ReservaAPI));
