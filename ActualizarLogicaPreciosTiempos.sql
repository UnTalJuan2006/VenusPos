-- Script para actualizar la configuración de precios y tiempos
-- Fecha: 2026-05-11
-- Descripción: Ajuste de multiplicadores según nuevos requerimientos

-- ============================================
-- MULTIPLICADORES DE TAMAÑO
-- ============================================

-- Perros pequeños con descuento del 10% (excepto doble capa)
UPDATE ConfiguracionesPrecios SET Valor = -0.10, Descripcion = 'Descuento 10% para perros pequeños pelo corto'
WHERE Clave = 'MULT_TAMANO_PEQUENO_CORTO';

INSERT INTO ConfiguracionesPrecios (Clave, Valor, Descripcion, FechaCreacion)
SELECT 'MULT_TAMANO_PEQUENO_CORTO', -0.10, 'Descuento 10% para perros pequeños pelo corto', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM ConfiguracionesPrecios WHERE Clave = 'MULT_TAMANO_PEQUENO_CORTO');

UPDATE ConfiguracionesPrecios SET Valor = -0.10, Descripcion = 'Descuento 10% para perros pequeños pelo semi largo'
WHERE Clave = 'MULT_TAMANO_PEQUENO_SEMI_LARGO';

INSERT INTO ConfiguracionesPrecios (Clave, Valor, Descripcion, FechaCreacion)
SELECT 'MULT_TAMANO_PEQUENO_SEMI_LARGO', -0.10, 'Descuento 10% para perros pequeños pelo semi largo', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM ConfiguracionesPrecios WHERE Clave = 'MULT_TAMANO_PEQUENO_SEMI_LARGO');

UPDATE ConfiguracionesPrecios SET Valor = -0.10, Descripcion = 'Descuento 10% para perros pequeños pelo largo'
WHERE Clave = 'MULT_TAMANO_PEQUENO_LARGO';

INSERT INTO ConfiguracionesPrecios (Clave, Valor, Descripcion, FechaCreacion)
SELECT 'MULT_TAMANO_PEQUENO_LARGO', -0.10, 'Descuento 10% para perros pequeños pelo largo', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM ConfiguracionesPrecios WHERE Clave = 'MULT_TAMANO_PEQUENO_LARGO');

-- Perro pequeño doble capa sin descuento
UPDATE ConfiguracionesPrecios SET Valor = 0.00, Descripcion = 'Sin recargo para perros pequeños doble capa'
WHERE Clave = 'MULT_TAMANO_PEQUENO_DOBLE_CAPA';

INSERT INTO ConfiguracionesPrecios (Clave, Valor, Descripcion, FechaCreacion)
SELECT 'MULT_TAMANO_PEQUENO_DOBLE_CAPA', 0.00, 'Sin recargo para perros pequeños doble capa', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM ConfiguracionesPrecios WHERE Clave = 'MULT_TAMANO_PEQUENO_DOBLE_CAPA');

-- Perros medianos: 20% para todos
UPDATE ConfiguracionesPrecios SET Valor = 0.20, Descripcion = 'Recargo 20% por tamaño mediano'
WHERE Clave = 'MULT_TAMANO_MEDIANO';

INSERT INTO ConfiguracionesPrecios (Clave, Valor, Descripcion, FechaCreacion)
SELECT 'MULT_TAMANO_MEDIANO', 0.20, 'Recargo 20% por tamaño mediano', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM ConfiguracionesPrecios WHERE Clave = 'MULT_TAMANO_MEDIANO');

-- Perros grandes: 50% para todos
UPDATE ConfiguracionesPrecios SET Valor = 0.50, Descripcion = 'Recargo 50% por tamaño grande'
WHERE Clave = 'MULT_TAMANO_GRANDE';

INSERT INTO ConfiguracionesPrecios (Clave, Valor, Descripcion, FechaCreacion)
SELECT 'MULT_TAMANO_GRANDE', 0.50, 'Recargo 50% por tamaño grande', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM ConfiguracionesPrecios WHERE Clave = 'MULT_TAMANO_GRANDE');

-- ============================================
-- MULTIPLICADORES DE PELAJE
-- ============================================

-- Perros pequeños: 0% (descuento ya aplicado en tamaño)
UPDATE ConfiguracionesPrecios SET Valor = 0.00, Descripcion = 'Sin recargo por pelaje para perros pequeños'
WHERE Clave = 'MULT_PELAJE_PEQUENO';

INSERT INTO ConfiguracionesPrecios (Clave, Valor, Descripcion, FechaCreacion)
SELECT 'MULT_PELAJE_PEQUENO', 0.00, 'Sin recargo por pelaje para perros pequeños', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM ConfiguracionesPrecios WHERE Clave = 'MULT_PELAJE_PEQUENO');

-- Perros medianos
UPDATE ConfiguracionesPrecios SET Valor = 0.00, Descripcion = 'Sin recargo por pelo corto en mediano'
WHERE Clave = 'MULT_PELAJE_MEDIANO_CORTO';

INSERT INTO ConfiguracionesPrecios (Clave, Valor, Descripcion, FechaCreacion)
SELECT 'MULT_PELAJE_MEDIANO_CORTO', 0.00, 'Sin recargo por pelo corto en mediano', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM ConfiguracionesPrecios WHERE Clave = 'MULT_PELAJE_MEDIANO_CORTO');

UPDATE ConfiguracionesPrecios SET Valor = 0.00, Descripcion = 'Sin recargo por pelo semi largo en mediano'
WHERE Clave = 'MULT_PELAJE_MEDIANO_SEMI_LARGO';

INSERT INTO ConfiguracionesPrecios (Clave, Valor, Descripcion, FechaCreacion)
SELECT 'MULT_PELAJE_MEDIANO_SEMI_LARGO', 0.00, 'Sin recargo por pelo semi largo en mediano', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM ConfiguracionesPrecios WHERE Clave = 'MULT_PELAJE_MEDIANO_SEMI_LARGO');

UPDATE ConfiguracionesPrecios SET Valor = 0.20, Descripcion = 'Recargo 20% por pelo largo en mediano'
WHERE Clave = 'MULT_PELAJE_MEDIANO_LARGO';

INSERT INTO ConfiguracionesPrecios (Clave, Valor, Descripcion, FechaCreacion)
SELECT 'MULT_PELAJE_MEDIANO_LARGO', 0.20, 'Recargo 20% por pelo largo en mediano', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM ConfiguracionesPrecios WHERE Clave = 'MULT_PELAJE_MEDIANO_LARGO');

UPDATE ConfiguracionesPrecios SET Valor = 0.40, Descripcion = 'Recargo 40% por doble capa en mediano'
WHERE Clave = 'MULT_PELAJE_MEDIANO_DOBLE_CAPA';

INSERT INTO ConfiguracionesPrecios (Clave, Valor, Descripcion, FechaCreacion)
SELECT 'MULT_PELAJE_MEDIANO_DOBLE_CAPA', 0.40, 'Recargo 40% por doble capa en mediano', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM ConfiguracionesPrecios WHERE Clave = 'MULT_PELAJE_MEDIANO_DOBLE_CAPA');

-- Perros grandes
UPDATE ConfiguracionesPrecios SET Valor = 0.00, Descripcion = 'Sin recargo por pelo corto en grande'
WHERE Clave = 'MULT_PELAJE_GRANDE_CORTO';

INSERT INTO ConfiguracionesPrecios (Clave, Valor, Descripcion, FechaCreacion)
SELECT 'MULT_PELAJE_GRANDE_CORTO', 0.00, 'Sin recargo por pelo corto en grande', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM ConfiguracionesPrecios WHERE Clave = 'MULT_PELAJE_GRANDE_CORTO');

UPDATE ConfiguracionesPrecios SET Valor = 0.00, Descripcion = 'Sin recargo por pelo semi largo en grande'
WHERE Clave = 'MULT_PELAJE_GRANDE_SEMI_LARGO';

INSERT INTO ConfiguracionesPrecios (Clave, Valor, Descripcion, FechaCreacion)
SELECT 'MULT_PELAJE_GRANDE_SEMI_LARGO', 0.00, 'Sin recargo por pelo semi largo en grande', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM ConfiguracionesPrecios WHERE Clave = 'MULT_PELAJE_GRANDE_SEMI_LARGO');

UPDATE ConfiguracionesPrecios SET Valor = 0.40, Descripcion = 'Recargo 40% por pelo largo en grande'
WHERE Clave = 'MULT_PELAJE_GRANDE_LARGO';

INSERT INTO ConfiguracionesPrecios (Clave, Valor, Descripcion, FechaCreacion)
SELECT 'MULT_PELAJE_GRANDE_LARGO', 0.40, 'Recargo 40% por pelo largo en grande', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM ConfiguracionesPrecios WHERE Clave = 'MULT_PELAJE_GRANDE_LARGO');

UPDATE ConfiguracionesPrecios SET Valor = 0.50, Descripcion = 'Recargo 50% por doble capa en grande'
WHERE Clave = 'MULT_PELAJE_GRANDE_DOBLE_CAPA';

INSERT INTO ConfiguracionesPrecios (Clave, Valor, Descripcion, FechaCreacion)
SELECT 'MULT_PELAJE_GRANDE_DOBLE_CAPA', 0.50, 'Recargo 50% por doble capa en grande', GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM ConfiguracionesPrecios WHERE Clave = 'MULT_PELAJE_GRANDE_DOBLE_CAPA');

-- ============================================
-- LIMPIAR CONFIGURACIONES OBSOLETAS
-- ============================================

DELETE FROM ConfiguracionesPrecios
WHERE Clave IN (
    'MULT_TAMANO_PEQUENO',
    'MULT_TAMANO_GRANDE_CORTO',
    'MULT_TAMANO_GRANDE_OTROS',
    'MULT_PELAJE_CORTO',
    'MULT_PELAJE_SEMI_LARGO',
    'MULT_PELAJE_LARGO',
    'MULT_PELAJE_DOBLE_CAPA',
    'MULT_PELAJE_SEMI_LARGO_MEDIANO',
    'MULT_PELAJE_LARGO_MEDIANO',
    'MULT_PELAJE_DOBLE_CAPA_MEDIANO',
    'MULT_PELAJE_SEMI_LARGO_GRANDE',
    'MULT_PELAJE_LARGO_GRANDE',
    'MULT_PELAJE_DOBLE_CAPA_GRANDE'
);

-- ============================================
-- VERIFICACIÓN
-- ============================================

SELECT
    Clave,
    Valor,
    Descripcion
FROM ConfiguracionesPrecios
WHERE Clave LIKE 'MULT_%'
ORDER BY
    CASE
        WHEN Clave LIKE '%PEQUENO%' THEN 1
        WHEN Clave LIKE '%MEDIANO%' THEN 2
        WHEN Clave LIKE '%GRANDE%' THEN 3
    END,
    Clave;

PRINT 'Configuración de precios actualizada correctamente';

DECLARE @TotalConfig INT;
SELECT @TotalConfig = COUNT(*) FROM ConfiguracionesPrecios WHERE Clave LIKE 'MULT_%';
PRINT 'Total de configuraciones: ' + CAST(@TotalConfig AS VARCHAR);
