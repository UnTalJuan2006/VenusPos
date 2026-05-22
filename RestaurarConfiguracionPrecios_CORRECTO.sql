-- ══════════════════════════════════════════════════════════════════
-- Script para restaurar la configuración de precios COMPLETA
-- ══════════════════════════════════════════════════════════════════
--
-- Este script inserta TODAS las claves que el código necesita
--
-- LÓGICA DE NEGOCIO:
--
-- PERROS PEQUEÑOS:
-- - Pelo corto: -10% (descuento)
-- - Pelo semi-largo: -10% (descuento)
-- - Pelo largo: -10% (descuento)
-- - Doble capa: 0% (sin descuento ni incremento)
--
-- PERROS MEDIANOS:
-- - Pelo corto: +20% (solo tamaño)
-- - Pelo semi-largo: +20% (solo tamaño)
-- - Pelo largo: +20% (tamaño) + 20% (pelaje) = +40%
-- - Doble capa: +20% (tamaño) + 40% (pelaje) = +60%
--
-- PERROS GRANDES:
-- - Pelo corto: +50% (solo tamaño)
-- - Pelo semi-largo: +50% (solo tamaño)
-- - Pelo largo: +50% (tamaño) + 40% (pelaje) = +90%
-- - Doble capa: +50% (tamaño) + 50% (pelaje) = +100%
--
-- ══════════════════════════════════════════════════════════════════

USE VenusPosDB;
GO

-- Eliminar configuraciones antiguas si existen
DELETE FROM ConfiguracionesPrecios WHERE Clave LIKE 'MULT_%';

-- Insertar TODAS las configuraciones necesarias
INSERT INTO ConfiguracionesPrecios (Clave, Valor, Descripcion, FechaCreacion)
VALUES
    -- ========================================
    -- MULTIPLICADORES DE TAMAÑO
    -- ========================================
    -- Pequeños (con descuento)
    ('MULT_TAMANO_PEQUENO_CORTO', -0.10, 'Multiplicador para tamaño pequeño pelo corto (-10%)', GETUTCDATE()),
    ('MULT_TAMANO_PEQUENO_SEMI_LARGO', -0.10, 'Multiplicador para tamaño pequeño pelo semi-largo (-10%)', GETUTCDATE()),
    ('MULT_TAMANO_PEQUENO_LARGO', -0.10, 'Multiplicador para tamaño pequeño pelo largo (-10%)', GETUTCDATE()),
    ('MULT_TAMANO_PEQUENO_DOBLE_CAPA', 0.00, 'Multiplicador para tamaño pequeño doble capa (sin cambio)', GETUTCDATE()),

    -- Medianos
    ('MULT_TAMANO_MEDIANO', 0.20, 'Multiplicador para tamaño mediano (+20%)', GETUTCDATE()),

    -- Grandes
    ('MULT_TAMANO_GRANDE', 0.50, 'Multiplicador para tamaño grande (+50%)', GETUTCDATE()),

    -- ========================================
    -- MULTIPLICADORES DE PELAJE
    -- ========================================
    -- Pequeños (0% porque el descuento ya está en tamaño)
    ('MULT_PELAJE_PEQUENO', 0.00, 'Multiplicador de pelaje para perros pequeños (sin incremento adicional)', GETUTCDATE()),

    -- Medianos
    ('MULT_PELAJE_MEDIANO_CORTO', 0.00, 'Multiplicador de pelaje para mediano pelo corto (sin incremento adicional)', GETUTCDATE()),
    ('MULT_PELAJE_MEDIANO_SEMI_LARGO', 0.00, 'Multiplicador de pelaje para mediano pelo semi-largo (sin incremento adicional)', GETUTCDATE()),
    ('MULT_PELAJE_MEDIANO_LARGO', 0.20, 'Multiplicador adicional de pelaje para mediano pelo largo (+20%)', GETUTCDATE()),
    ('MULT_PELAJE_MEDIANO_DOBLE_CAPA', 0.40, 'Multiplicador adicional de pelaje para mediano doble capa (+40%)', GETUTCDATE()),

    -- Grandes
    ('MULT_PELAJE_GRANDE_CORTO', 0.00, 'Multiplicador de pelaje para grande pelo corto (sin incremento adicional)', GETUTCDATE()),
    ('MULT_PELAJE_GRANDE_SEMI_LARGO', 0.00, 'Multiplicador de pelaje para grande pelo semi-largo (sin incremento adicional)', GETUTCDATE()),
    ('MULT_PELAJE_GRANDE_LARGO', 0.40, 'Multiplicador adicional de pelaje para grande pelo largo (+40%)', GETUTCDATE()),
    ('MULT_PELAJE_GRANDE_DOBLE_CAPA', 0.50, 'Multiplicador adicional de pelaje para grande doble capa (+50%)', GETUTCDATE());

GO

-- Verificar las configuraciones insertadas
SELECT
    Id,
    Clave,
    CAST(Valor * 100 AS VARCHAR(10)) + '%' AS Porcentaje,
    Descripcion,
    FechaCreacion
FROM ConfiguracionesPrecios
WHERE Clave LIKE 'MULT_%'
ORDER BY
    CASE
        WHEN Clave LIKE '%PEQUENO%' THEN 1
        WHEN Clave LIKE '%MEDIANO%' THEN 2
        WHEN Clave LIKE '%GRANDE%' THEN 3
        ELSE 4
    END,
    Clave;

PRINT '✓ Configuraciones de precios restauradas correctamente';
PRINT '✓ Total de configuraciones: 15';
GO
