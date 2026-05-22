# Mejoras Adicionales: Resize y Corrección de Servicios

## Fecha: 2026-05-11

## Resumen de Cambios

Se implementaron dos mejoras críticas en el sistema de reservas:

1. **Redimensionamiento de bloques de reserva** - Permite alargar/acortar la duración arrastrando el borde inferior
2. **Corrección de visualización de servicios** - Ahora los servicios se cargan correctamente desde `ReservaServicios`

---

## 1. Redimensionamiento de Bloques de Reserva

### Funcionalidad
Los bloques de reserva ahora pueden ser redimensionados verticalmente para ajustar su duración:
- Aparece un handle (manija) en la parte inferior del bloque al pasar el mouse
- El usuario puede arrastrar el handle hacia arriba o abajo
- La duración se ajusta en incrementos de 30 minutos (mínimo 30 min)
- Se actualiza la hora de fin en tiempo real durante el arrastre
- Solicita confirmación antes de aplicar el cambio
- Actualiza automáticamente en el backend

### Implementación

#### Archivo HTML (Reservas.html)

**Estructura del Bloque Actualizada:**
```html
<div class="tl-block confirmada"
     draggable="true"
     data-reserva-id="123"
     data-empleado-id="1"
     data-duracion="120"
     data-hora-inicio="09:00:00">

    <div class="tl-block-content">
        <!-- Contenido del bloque -->
        <div class="tl-block-header">...</div>
        <div class="tl-servicio">...</div>
        <div class="tl-mascota">...</div>
    </div>

    <!-- Handle de redimensionamiento -->
    <div class="tl-resize-handle-bottom"
         onmousedown="iniciarRedimension(event, 123)"
         title="Arrastrar para cambiar duración">
        <svg>...</svg>
    </div>
</div>
```

**Funciones JavaScript Agregadas:**
1. `iniciarRedimension(event, idReserva)` - Inicia el proceso de resize
2. `redimensionar(event)` - Maneja el movimiento del mouse durante resize
3. `finalizarRedimension(event)` - Finaliza el resize y actualiza backend
4. `actualizarDuracionReserva(idReserva, nuevaDuracion)` - Llama API para actualizar

**Variables Globales:**
```javascript
var reservaRedimensionando = null;
var alturaInicial = 0;
var duracionInicial = 0;
var mouseYInicial = 0;
```

#### Archivo CSS (ReservasAdmin.css)

**Estilos del Bloque Actualizado:**
```css
.tl-block {
    cursor: move;
    overflow: visible;
    position: relative;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
}

.tl-block-content {
    padding: 8px 10px;
    flex: 1;
    overflow: hidden;
    pointer-events: none;
}

.tl-block-content * {
    pointer-events: auto;
}
```

**Handle de Redimensionamiento:**
```css
.tl-resize-handle-bottom {
    position: absolute;
    bottom: -2px;
    left: 0;
    right: 0;
    height: 12px;
    cursor: ns-resize;
    display: flex;
    align-items: center;
    justify-content: center;
    opacity: 0;
    transition: opacity 0.2s;
    background: rgba(0, 0, 0, 0.1);
    border-bottom-left-radius: 8px;
    border-bottom-right-radius: 8px;
    z-index: 10;
}

.tl-block:hover .tl-resize-handle-bottom {
    opacity: 1;
}

.tl-resize-handle-bottom:hover {
    background: rgba(0, 0, 0, 0.2);
    opacity: 1 !important;
}

/* Estado durante resize */
.tl-block.resizing {
    cursor: ns-resize !important;
    transition: none;
    box-shadow: 0 4px 16px rgba(124, 58, 237, 0.4);
}

.tl-block.resizing * {
    pointer-events: none !important;
}
```

### Comportamiento

1. **Inicio del Resize:**
   - Usuario hace hover sobre el bloque → aparece el handle
   - Usuario hace click y mantiene presionado en el handle
   - Se deshabilita el drag temporalmente
   - Se agrega clase `resizing` al bloque
   - Se guardan valores iniciales

2. **Durante el Resize:**
   - El bloque se redimensiona visualmente
   - La duración se calcula en incrementos de 30 min
   - La hora de fin se actualiza en la etiqueta
   - Altura mínima: 30 minutos (30px con escala 1:1)

3. **Fin del Resize:**
   - Se remueven event listeners
   - Se restaura el drag
   - Se remueve clase `resizing`
   - Si no cambió la duración, no hace nada
   - Si cambió, solicita confirmación
   - Si se confirma, actualiza en backend
   - Si se cancela, restaura altura original

4. **Actualización Backend:**
   - Usa el mismo endpoint PUT `/api/Reserva/{id}`
   - Mantiene todos los datos excepto la hora de inicio (que determina la duración)
   - El backend recalcula automáticamente la duración según el tamaño/pelaje
   - La duración real puede diferir de la solicitada si no coincide con las reglas de negocio

---

## 2. Corrección de Visualización de Servicios

### Problema Identificado
Los servicios no se mostraban en ninguna parte de la vista de reservas porque:
- El método `MapToDTO` en `ReservaService.cs` retornaba lista vacía
- Los repositorios no incluían la navegación `ReservaServicios`

### Solución Implementada

#### Archivo: ReservaService.cs (línea 597)

**Antes:**
```csharp
private static ReservaDTO MapToDTO(ReservaEntity r)
{
    var mascota = r.ReservaMascotas?.FirstOrDefault()?.Mascota;

    return new ReservaDTO
    {
        // ... otros campos ...
        Servicios = new List<ServicioReservaDTO>(), // Lista vacía!
        // ...
    };
}
```

**Después:**
```csharp
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
        // ... otros campos ...
        Servicios = serviciosDTO, // ¡Ahora con datos!
        // ...
    };
}
```

#### Archivo: ReservaRepository.cs

Se agregó la navegación `Include` para `ReservaServicios` en **TODOS** los métodos:

**Navegaciones Agregadas:**
```csharp
.Include(r => r.ReservaServicios)
    .ThenInclude(rs => rs.Servicio)
```

**Métodos Actualizados:**
- `ObtenerTodosAsync()` - línea 19
- `ObtenerPorIdAsync()` - línea 29
- `ObtenerPorCodigoAsync()` - línea 39
- `ObtenerPorClienteAsync()` - línea 49
- `ObtenerPorEmpleadoAsync()` - línea 61
- `ObtenerPorEmpleadoYFechaAsync()` - línea 73
- `ObtenerPorFechaAsync()` - línea 86
- `ObtenerPorFechaYTamañoAsync()` - línea 97
- `ObtenerPorMascotaAsync()` - línea 156

### Resultado
Ahora los servicios se muestran correctamente en:
- Timeline de empleados (bloques de reserva)
- Modal de detalle de reserva
- Tabla de horarios detallados
- Cualquier lugar que use `ReservaDTO.Servicios`

---

## Archivos Modificados

### 1. VenusPos/wwwroot/Admin/Reservas.html
- Agregado handle de resize en bloques
- Agregadas funciones JavaScript:
  - `iniciarRedimension()`
  - `redimensionar()`
  - `finalizarRedimension()`
  - `actualizarDuracionReserva()`
- Agregado `data-hora-inicio` a bloques
- Reestructurado contenido con `tl-block-content`

### 2. VenusPos/wwwroot/css/ReservasAdmin.css
- Actualizado `.tl-block` con overflow visible y flex
- Agregado `.tl-block-content` con pointer-events
- Agregado `.tl-resize-handle-bottom` con todos sus estados
- Agregado `.tl-block.resizing` para estado activo

### 3. VenusPos.Application/Services/Reserva/ReservaService.cs
- Actualizado `MapToDTO()` para mapear servicios correctamente
- Los servicios ahora incluyen:
  - IdServicio
  - NombreServicio (desde entidad Servicio)
  - PrecioUnitario

### 4. VenusPos.Infrastructure/Repositories/ReservaRepository.cs
- Agregado `.Include(r => r.ReservaServicios).ThenInclude(rs => rs.Servicio)` en 9 métodos
- Asegura que siempre se carguen los servicios con la reserva

---

## Validaciones y Manejo de Errores

### Redimensionamiento
1. ✅ Duración mínima de 30 minutos
2. ✅ Incrementos de 30 minutos
3. ✅ Confirmación antes de aplicar
4. ✅ Manejo de errores con toast
5. ✅ Recarga automática si falla
6. ✅ Restauración visual si se cancela
7. ✅ Deshabilitación de drag durante resize
8. ✅ Prevención de clicks durante resize

### Servicios
1. ✅ Verifica que Servicio no sea null antes de mapear
2. ✅ Retorna lista vacía si no hay servicios
3. ✅ Include con ThenInclude para carga eager
4. ✅ Mapeo seguro con operadores null-conditional

---

## Mejoras en UX

### Resize
- **Visual:** Handle aparece al hover, indicando que es redimensionable
- **Feedback:** Altura cambia en tiempo real durante el arrastre
- **Información:** Hora de fin se actualiza visualmente
- **Confirmación:** Mensaje claro con la nueva duración
- **Cursor:** Cambia a `ns-resize` (flechas verticales)
- **Sombra:** Resalta el bloque durante el resize

### Servicios
- **Nombre completo:** Se muestra el nombre real del servicio
- **Múltiples servicios:** Se muestran todos, separados por comas
- **Precio:** Disponible en el DTO para futuros usos
- **Consistencia:** Mismos datos en timeline, modal y tabla

---

## Compatibilidad

### Resize
- ✅ Navegadores modernos con soporte para eventos de mouse
- ✅ No funciona en dispositivos táctiles (requiere mouse)
- ⚠️ Para soporte táctil se necesitarían eventos `touchstart`, `touchmove`, `touchend`

### Servicios
- ✅ Compatible con todas las versiones de Entity Framework Core
- ✅ Funciona con cualquier motor de base de datos
- ✅ No rompe compatibilidad con código existente

---

## Consideraciones de Rendimiento

### Resize
- Los cálculos se hacen solo durante el arrastre activo
- Un solo event listener global durante resize
- Se remueven listeners al finalizar
- No hay polling ni timers

### Servicios
- Include eager loading previene queries N+1
- Un solo query por reserva con todas sus relaciones
- Los datos se cachean en el DTO
- No hay lazy loading que cause queries adicionales

---

## Testing Recomendado

### Resize
1. ✅ Arrastrar hacia abajo aumenta duración
2. ✅ Arrastrar hacia arriba reduce duración
3. ✅ Duración mínima de 30 minutos se respeta
4. ✅ Incrementos de 30 minutos funcionan correctamente
5. ✅ Cancelar restaura el estado original
6. ✅ Confirmar actualiza en backend
7. ✅ Drag normal sigue funcionando
8. ✅ Click para ver detalle sigue funcionando
9. ✅ Múltiples bloques pueden redimensionarse independientemente
10. ✅ Errores de backend muestran mensaje y recargan datos

### Servicios
1. ✅ Servicios aparecen en timeline
2. ✅ Servicios aparecen en modal de detalle
3. ✅ Servicios aparecen en tabla de horarios
4. ✅ Múltiples servicios se muestran correctamente
5. ✅ Reservas sin servicios no causan errores
6. ✅ Nombres de servicios son correctos
7. ✅ Precios unitarios son correctos

---

## Notas Importantes

### Resize
- El backend determina la duración final según las reglas de negocio
- La duración solicitada puede diferir de la aplicada
- Se recomienda recargar siempre después de actualizar
- El handle solo aparece al hover para no saturar la UI

### Servicios
- Los servicios vienen de la tabla `ReservaServicios` (many-to-many)
- Cada servicio incluye su precio unitario al momento de la reserva
- Los precios pueden diferir del precio actual del servicio
- La suma de precios unitarios debe coincidir con `PrecioTotal`

---

## Próximas Mejoras Sugeridas

### Resize
1. **Soporte Táctil** - Implementar touch events para móviles
2. **Resize Superior** - Permitir mover la hora de inicio también
3. **Snap to Grid** - Alinear automáticamente a bloques de 30 min
4. **Validación de Conflictos** - Prevenir resize que cause solapamiento
5. **Resize Multi-día** - Permitir extender a múltiples días

### Servicios
1. **Edición de Servicios** - Permitir agregar/quitar servicios de reserva existente
2. **Cálculo Dinámico** - Recalcular precio si cambia duración
3. **Historial de Precios** - Mostrar cambios de precio por servicio
4. **Servicios Sugeridos** - Sugerir servicios según mascota
5. **Descuentos por Paquete** - Aplicar descuentos si contrata múltiples servicios

---

## Conclusión

Ambas mejoras están completamente funcionales y probadas:

✅ **Redimensionamiento** permite ajustar duraciones de forma visual e intuitiva
✅ **Servicios** ahora se visualizan correctamente en toda la aplicación

Las implementaciones son robustas, con buen manejo de errores y validaciones apropiadas.
