# Resumen de Cambios en Lógica de Precios y Duraciones

## Fecha de actualización
Mayo 7, 2026

## Nueva Lógica de Precios

### Perros Pequeños (sin cambios)
Mantienen la lógica anterior:
- Pelo corto: 0% adicional
- Pelo semi-largo: +15% adicional
- Pelo largo: +25% adicional
- Doble capa: +45% adicional

### Perros Medianos
**Multiplicador de tamaño:** +20% (aplicado a todos los tipos de pelaje)

**Multiplicadores de pelaje adicionales:**
- Pelo corto: +20% (solo tamaño) = **+20% total**
- Pelo semi-largo: +20% (tamaño) + 0% (pelaje) = **+20% total** - Duración: 2.5 horas
- Pelo largo: +20% (tamaño) + 0% (pelaje) = **+20% total** - Duración: 2.5 horas
- Doble capa: +20% (tamaño) + 25% (doble capa) = **+45% total** - Duración: 3 horas

### Perros Grandes
**Multiplicadores de tamaño:**
- Pelo corto: +50%
- Otros pelajes (semi-largo, largo, doble capa): +30%

**Multiplicadores de pelaje adicionales:**
- Pelo corto: +50% (solo tamaño) = **+50% total** - Duración: 3 horas
- Pelo semi-largo: +30% (tamaño) + 25% (semi-largo) = **+55% total** - Duración: 3.5 horas
- Pelo largo: +30% (tamaño) + 30% (largo) = **+60% total** - Duración: 3.5 horas
- Doble capa: +30% (tamaño) + 35% (doble capa) = **+65% total** - Duración: 4 horas

## Cambios en Duraciones

### Perros Pequeños (sin cambios)
- Pelo corto: 1 hora (60 min)
- Pelo semi-largo: 1.5 horas (90 min)
- Pelo largo: 2 horas (120 min)
- Doble capa: 2 horas (120 min)

### Perros Medianos
- Pelo corto: 2 horas (120 min)
- Pelo semi-largo: 2.5 horas (150 min)
- Pelo largo: 2.5 horas (150 min) - **CAMBIO:** antes era 3 horas
- Doble capa: 3 horas (180 min)

### Perros Grandes
- Pelo corto: 3 horas (180 min)
- Pelo semi-largo: 3.5 horas (210 min)
- Pelo largo: 3.5 horas (210 min) - **CAMBIO:** antes era 4 horas
- Doble capa: 4 horas (240 min)

## Archivos Modificados

### 1. Base de Datos
- **ActualizarConfiguracionPrecios.sql**: Script de actualización con nuevas configuraciones
- **seed_reservas.sql**: Datos iniciales actualizados

### 2. Código C#
- **VenusPos.Application/Services/Reserva/ReservaService.cs**:
  - Método `CalcularDuracionMinutos`: Actualizado con nuevas duraciones
  - Método `ObtenerMultiplicadorTamañoAsync`: Ahora considera el tipo de pelaje para perros grandes
  - Método `ObtenerMultiplicadorPelajeAsync`: Ahora aplica lógica diferenciada por tamaño

## Nuevas Claves de Configuración en Base de Datos

### Tamaños
- `MULT_TAMANO_PEQUENO`: 0.00 (sin cambios)
- `MULT_TAMANO_MEDIANO`: 0.20 (sin cambios)
- `MULT_TAMANO_GRANDE_CORTO`: 0.50 (nueva - antes era MULT_TAMANO_GRANDE)
- `MULT_TAMANO_GRANDE_OTROS`: 0.30 (nueva)

### Pelajes - Perros Pequeños (sin cambios)
- `MULT_PELAJE_CORTO`: 0.00
- `MULT_PELAJE_SEMI_LARGO`: 0.15
- `MULT_PELAJE_LARGO`: 0.25
- `MULT_PELAJE_DOBLE_CAPA`: 0.45

### Pelajes - Perros Medianos (nuevas)
- `MULT_PELAJE_SEMI_LARGO_MEDIANO`: 0.00
- `MULT_PELAJE_LARGO_MEDIANO`: 0.00
- `MULT_PELAJE_DOBLE_CAPA_MEDIANO`: 0.25

### Pelajes - Perros Grandes (nuevas)
- `MULT_PELAJE_SEMI_LARGO_GRANDE`: 0.25
- `MULT_PELAJE_LARGO_GRANDE`: 0.30
- `MULT_PELAJE_DOBLE_CAPA_GRANDE`: 0.35

## Pasos para Aplicar los Cambios

1. **Ejecutar el script SQL de actualización:**
   ```sql
   -- Ejecutar el archivo: ActualizarConfiguracionPrecios.sql
   ```
   Este script actualizará las configuraciones existentes y agregará las nuevas.

2. **Compilar y ejecutar la aplicación:**
   ```bash
   dotnet build
   dotnet run
   ```

3. **Verificar que los cálculos sean correctos:**
   - Crear una reserva con un perro mediano de pelo largo
   - Verificar que el precio sea +20% del precio base
   - Verificar que la duración sea 2.5 horas (150 minutos)

## Ejemplos de Cálculo

### Ejemplo 1: Perro Mediano - Pelo Largo
- Precio base del servicio: $50,000
- Multiplicador tamaño: +20% = $10,000
- Multiplicador pelaje: +0% = $0
- **Precio final: $60,000**
- **Duración: 2.5 horas (150 minutos)**

### Ejemplo 2: Perro Grande - Doble Capa
- Precio base del servicio: $50,000
- Multiplicador tamaño: +30% = $15,000
- Multiplicador pelaje: +35% = $17,500
- **Precio final: $82,500**
- **Duración: 4 horas (240 minutos)**

### Ejemplo 3: Perro Grande - Pelo Corto
- Precio base del servicio: $50,000
- Multiplicador tamaño: +50% = $25,000
- Multiplicador pelaje: +0% = $0
- **Precio final: $75,000**
- **Duración: 3 horas (180 minutos)**
