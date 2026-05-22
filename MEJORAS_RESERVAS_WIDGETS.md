# Mejoras Implementadas en Vista de Reservas

## Fecha: 2026-05-11

## Resumen de Funcionalidades Implementadas

Se han implementado tres mejoras importantes en la vista de Reservas (Reservas.html) para mejorar la experiencia del usuario al gestionar reservas desde los widgets de calendario.

---

## 1. Click en Calendario para Crear Reserva Rápida

### Funcionalidad
- Al hacer click en cualquier franja horaria del timeline de un empleado, se abre el modal de creación de reserva con datos pre-llenados
- La fecha, hora y empleado se establecen automáticamente según la celda clickeada

### Implementación
- **Archivo:** `VenusPos/wwwroot/Admin/Reservas.html`
- **Función:** `crearReservaRapida(element)`
- **Cambios en HTML:**
  - Las celdas del timeline ahora tienen clase `timeline-clickeable`
  - Atributos `data-hora` y `data-empleado` para identificar el slot
  - Evento `onclick="crearReservaRapida(this)"`

### CSS Actualizado
```css
.timeline-clickeable {
    cursor: pointer;
    transition: background 0.2s;
}

.timeline-clickeable:hover {
    background: var(--purple-soft);
}
```

---

## 2. Modal de Detalle de Reserva

### Funcionalidad
- Al hacer click sobre un bloque de reserva en el timeline, se muestra un modal con información completa
- Muestra: código, fecha/hora, cliente, mascota, empleado, servicios, duración, precio total y observaciones
- Incluye botón para editar (en desarrollo)

### Implementación
- **Archivo:** `VenusPos/wwwroot/Admin/Reservas.html`
- **Función:** `verDetallesReserva(id)`
- **Función:** `cerrarModalDetalle(event)`
- **Nuevo Modal:** `modalDetalleOverlay`

### Estructura del Modal
```html
<div class="modal-overlay" id="modalDetalleOverlay">
    <div class="modal modal-detalle">
        <div class="modal-hdr">
            <h3 class="modal-ttl">Detalle de Reserva</h3>
            <button class="modal-close">...</button>
        </div>
        <div class="modal-body" id="modalDetalleBody">
            <!-- Contenido dinámico -->
        </div>
        <div class="modal-footer">
            <button class="btn-sec">Cerrar</button>
            <button class="btn-primary">Editar</button>
        </div>
    </div>
</div>
```

### CSS Agregado
```css
.detalle-reserva { ... }
.detalle-header { ... }
.detalle-codigo { ... }
.detalle-section { ... }
.detalle-label { ... }
.detalle-value { ... }
.detalle-precio { ... }
```

---

## 3. Drag and Drop para Mover Reservas

### Funcionalidad
- Los bloques de reserva son arrastrables (draggable)
- Puedes arrastrar una reserva a una nueva franja horaria
- Funciona también para cambiar de empleado (arrastrar a otra columna)
- Muestra indicadores visuales durante el arrastre
- Solicita confirmación antes de aplicar el cambio
- Actualiza la reserva en el backend automáticamente

### Implementación

#### Archivos Modificados:
1. **VenusPos/wwwroot/Admin/Reservas.html**
   - Funciones: `iniciarArrastre()`, `finalizarArrastre()`, `permitirDrop()`, `soltarReserva()`
   - Función: `actualizarHorarioReserva(idReserva, nuevaHora, nuevoEmpleadoId)`

2. **VenusPos.Application/DTOs/Reserva/ActualizarReservaDTO.cs** (NUEVO)
   - DTO para actualizar reservas completas

3. **VenusPos.Application/Interfaces/Services/IReservaService.cs**
   - Método: `Task<ReservaDTO> ActualizarReservaAsync(int id, ActualizarReservaDTO dto)`

4. **VenusPos.Application/Services/Reserva/ReservaService.cs**
   - Implementación completa de `ActualizarReservaAsync()`
   - Valida disponibilidad del nuevo horario
   - Recalcula precio y duración
   - Actualiza todas las relaciones

5. **VenusPos/Controllers/ReservaController.cs**
   - Endpoint: `PUT /api/Reserva/{id}`

### Atributos HTML de Bloques Draggables
```html
<div class="tl-block confirmada"
     draggable="true"
     data-reserva-id="123"
     data-empleado-id="1"
     data-duracion="120"
     ondragstart="iniciarArrastre(event)"
     ondragend="finalizarArrastre(event)"
     style="cursor: move;">
```

### CSS para Drag and Drop
```css
.tl-block {
    cursor: move;
    transition: transform 0.15s, box-shadow 0.15s, opacity 0.15s;
}

.tl-block:active {
    cursor: grabbing;
}

.drop-zone-active {
    border-top: 2px dashed var(--purple) !important;
}

.drop-zone-hover {
    background: var(--purple-soft) !important;
    border-top: 2px solid var(--purple) !important;
}
```

### Flujo del Drag and Drop
1. Usuario inicia arrastre → `iniciarArrastre(event)`
2. Se agregan indicadores visuales a todas las celdas
3. Usuario arrastra sobre una celda → `permitirDrop(event)`
4. Usuario suelta → `soltarReserva(event)`
5. Se solicita confirmación
6. Se llama `actualizarHorarioReserva()` que hace PUT a la API
7. Se recargan los datos para reflejar el cambio en todos los widgets

---

## Validaciones Implementadas

### En el Backend (ReservaService.cs)
- ✅ Validar que la reserva existe
- ✅ Validar que la mascota pertenece al cliente
- ✅ Validar que el empleado existe y está activo
- ✅ Validar disponibilidad del nuevo horario (si cambió)
- ✅ Recalcular duración según tamaño y pelaje de la mascota
- ✅ Recalcular precio según servicios y características
- ✅ Actualizar todas las relaciones (mascotas, servicios)

### En el Frontend
- ✅ Confirmación antes de mover reserva
- ✅ Manejo de errores con mensajes toast
- ✅ Recarga automática de datos después de actualizar
- ✅ Indicadores visuales durante el arrastre
- ✅ Prevención de drops inválidos

---

## Archivos Modificados

### Nuevos Archivos
1. `VenusPos.Application/DTOs/Reserva/ActualizarReservaDTO.cs`
2. `MEJORAS_RESERVAS_WIDGETS.md` (este documento)

### Archivos Modificados
1. `VenusPos/wwwroot/Admin/Reservas.html`
   - Agregado modal de detalle
   - Funciones de creación rápida
   - Sistema completo de drag and drop
   - Funciones de actualización de reservas

2. `VenusPos/wwwroot/css/ReservasAdmin.css`
   - Estilos para timeline clickeable
   - Estilos para drag and drop
   - Estilos para modal de detalle
   - Mejoras en cursor y transiciones

3. `VenusPos.Application/Interfaces/Services/IReservaService.cs`
   - Agregado método `ActualizarReservaAsync()`

4. `VenusPos.Application/Services/Reserva/ReservaService.cs`
   - Implementación completa de actualización de reservas

5. `VenusPos/Controllers/ReservaController.cs`
   - Endpoint PUT `/api/Reserva/{id}`

---

## Endpoints API Utilizados

### Existentes
- `GET /api/Reserva` - Obtener todas las reservas
- `GET /api/Empleado` - Obtener empleados
- `GET /api/Servicio` - Obtener servicios

### Nuevo
- `PUT /api/Reserva/{id}` - Actualizar reserva completa
  - **Body:** ActualizarReservaDTO
  - **Respuesta:** ReservaDTO actualizado

---

## Experiencia de Usuario

### Antes
- Solo se podía crear reservas desde el botón "Nueva Reserva"
- Ver detalles mostraba un alert() simple
- No se podían reorganizar reservas una vez creadas

### Después
- ✨ Click rápido en cualquier hora para crear reserva
- ✨ Modal elegante con toda la información de la reserva
- ✨ Arrastrar y soltar para reorganizar horarios
- ✨ Cambio automático de empleado arrastrando entre columnas
- ✨ Actualización en tiempo real en todos los widgets
- ✨ Confirmaciones intuitivas antes de aplicar cambios

---

## Próximas Mejoras Sugeridas

1. **Función de Edición Completa**
   - Actualmente el botón "Editar" en el modal está en desarrollo
   - Implementar formulario completo de edición

2. **Arrastre Vertical Preciso**
   - Permitir arrastrar a medias horas (no solo horas completas)
   - Snapping más preciso

3. **Vista de Colisiones**
   - Marcar visualmente cuando dos reservas se solapan
   - Advertencias antes de crear conflictos

4. **Historial de Cambios**
   - Registrar quién movió/cambió una reserva
   - Auditoría de cambios de horario

---

## Testing Recomendado

### Casos de Prueba
1. ✅ Click en franja horaria disponible abre modal con datos correctos
2. ✅ Click en reserva muestra modal de detalle con información completa
3. ✅ Arrastrar reserva dentro del mismo empleado actualiza horario
4. ✅ Arrastrar reserva a otro empleado cambia asignación
5. ✅ Validación de disponibilidad previene conflictos
6. ✅ Cancelar durante drag restaura estado original
7. ✅ Actualización se refleja en todos los widgets simultáneamente
8. ✅ Manejo de errores muestra mensajes apropiados

---

## Notas Técnicas

- Se utiliza HTML5 Drag and Drop API nativo
- Los eventos `dragstart`, `dragover`, `drop` y `dragend` gestionan el flujo
- Las reservas mantienen integridad referencial con mascotas y servicios
- La recalculación de precio es automática según las reglas de negocio
- Todos los cambios requieren confirmación del usuario
- La actualización es transaccional (todo o nada)

---

## Compatibilidad

- ✅ Navegadores modernos (Chrome, Firefox, Edge, Safari)
- ✅ Responsive (funciona en tablets)
- ⚠️ Drag and drop limitado en móviles (táctil requiere implementación adicional)
- ✅ Modo claro y oscuro soportado
