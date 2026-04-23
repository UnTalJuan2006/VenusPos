-- Script para actualizar la configuración de precios
-- Actualizar multiplicadores de tamaño y pelaje

-- ============================================
-- MULTIPLICADORES DE TAMAÑO
-- ============================================

-- Cambiar el multiplicador de tamaño mediano a 20%
UPDATE ConfiguracionesPrecios
SET Valor = 0.20,
    Descripcion = 'Multiplicador para tamaño mediano (20%)',
    FechaActualizacion = GETDATE()
WHERE Clave = 'MULT_TAMANO_MEDIANO';

-- Cambiar el multiplicador de tamaño grande a 50%
UPDATE ConfiguracionesPrecios
SET Valor = 0.50,
    Descripcion = 'Multiplicador para tamaño grande (50%)',
    FechaActualizacion = GETDATE()
WHERE Clave = 'MULT_TAMANO_GRANDE';

-- ============================================
-- MULTIPLICADORES DE PELAJE
-- ============================================

-- Insertar o actualizar multiplicador de pelaje corto (0%)
IF NOT EXISTS (SELECT 1 FROM ConfiguracionesPrecios WHERE Clave = 'MULT_PELAJE_CORTO')
BEGIN
    INSERT INTO ConfiguracionesPrecios (Clave, Valor, Descripcion, FechaCreacion)
    VALUES ('MULT_PELAJE_CORTO', 0.00, 'Multiplicador para pelaje corto (sin incremento)', GETDATE());
END
ELSE
BEGIN
    UPDATE ConfiguracionesPrecios
    SET Valor = 0.00,
        Descripcion = 'Multiplicador para pelaje corto (sin incremento)',
        FechaActualizacion = GETDATE()
    WHERE Clave = 'MULT_PELAJE_CORTO';
END

-- Insertar o actualizar multiplicador de pelaje semi largo (15%)
IF NOT EXISTS (SELECT 1 FROM ConfiguracionesPrecios WHERE Clave = 'MULT_PELAJE_SEMI_LARGO')
BEGIN
    INSERT INTO ConfiguracionesPrecios (Clave, Valor, Descripcion, FechaCreacion)
    VALUES ('MULT_PELAJE_SEMI_LARGO', 0.15, 'Multiplicador para pelaje semi largo (15%)', GETDATE());
END
ELSE
BEGIN
    UPDATE ConfiguracionesPrecios
    SET Valor = 0.15,
        Descripcion = 'Multiplicador para pelaje semi largo (15%)',
        FechaActualizacion = GETDATE()
    WHERE Clave = 'MULT_PELAJE_SEMI_LARGO';
END

-- Insertar o actualizar multiplicador de pelaje largo (25%)
IF NOT EXISTS (SELECT 1 FROM ConfiguracionesPrecios WHERE Clave = 'MULT_PELAJE_LARGO')
BEGIN
    INSERT INTO ConfiguracionesPrecios (Clave, Valor, Descripcion, FechaCreacion)
    VALUES ('MULT_PELAJE_LARGO', 0.25, 'Multiplicador para pelaje largo (25%)', GETDATE());
END
ELSE
BEGIN
    UPDATE ConfiguracionesPrecios
    SET Valor = 0.25,
        Descripcion = 'Multiplicador para pelaje largo (25%)',
        FechaActualizacion = GETDATE()
    WHERE Clave = 'MULT_PELAJE_LARGO';
END

-- Cambiar el multiplicador de pelaje doble capa a 45%
UPDATE ConfiguracionesPrecios
SET Valor = 0.45,
    Descripcion = 'Multiplicador para pelaje doble capa (45%)',
    FechaActualizacion = GETDATE()
WHERE Clave = 'MULT_PELAJE_DOBLE_CAPA';

-- ============================================
-- VERIFICAR LOS CAMBIOS
-- ============================================

SELECT Clave, Valor, Descripcion,
       COALESCE(FechaActualizacion, FechaCreacion) AS UltimaModificacion
FROM ConfiguracionesPrecios
WHERE Clave LIKE 'MULT_%'
ORDER BY Clave;
