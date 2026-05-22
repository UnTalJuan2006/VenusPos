-- ══════════════════════════════════════════════════════════════════
-- Script para actualizar configuración de precios según tipo de pelaje
-- ══════════════════════════════════════════════════════════════════
--
-- Lógica de negocio ACTUALIZADA:
--
-- PERROS MEDIANOS (MULT_TAMANO_MEDIANO = 20%):
-- - Pelo corto: +20% (solo tamaño)
-- - Pelo semi-largo: +20% (tamaño) - Duración: 2.5 horas
-- - Pelo largo: +20% (tamaño) - Duración: 2.5 horas
-- - Doble capa: +20% (tamaño) + 25% (doble capa) = +45% - Duración: 3 horas
--
-- PERROS GRANDES:
-- - Pelo corto: +50% (solo tamaño) - Duración normal (3 horas)
-- - Pelo semi-largo: +30% (tamaño) + 25% (semi-largo) = +55% - Duración: 3.5 horas
-- - Pelo largo: +30% (tamaño) + 30% (largo) = +60% - Duración: 3.5 horas
-- - Doble capa: +30% (tamaño) + 35% (doble capa) = +65% - Duración: 4 horas
--
-- ══════════════════════════════════════════════════════════════════

USE VenusPosDB;
GO

-- Actualizar o insertar configuraciones de tamaño y pelaje
MERGE INTO ConfiguracionesPrecios AS target
USING (VALUES
    -- Tamaños
    ('MULT_TAMANO_PEQUENO', 0.00, 'Multiplicador para tamaño pequeño (sin incremento)'),
    ('MULT_TAMANO_MEDIANO', 0.20, 'Multiplicador para tamaño mediano (+20%)'),
    ('MULT_TAMANO_GRANDE_CORTO', 0.50, 'Multiplicador para tamaño grande pelo corto (+50%)'),
    ('MULT_TAMANO_GRANDE_OTROS', 0.30, 'Multiplicador para tamaño grande pelo semi-largo/largo/doble capa (+30%)'),

    -- Pelajes (se aplican adicionalmente al multiplicador de tamaño)
    ('MULT_PELAJE_CORTO', 0.00, 'Multiplicador para pelo corto (sin incremento adicional)'),
    ('MULT_PELAJE_SEMI_LARGO_MEDIANO', 0.00, 'Multiplicador para pelo semi-largo en perros medianos (sin incremento adicional)'),
    ('MULT_PELAJE_LARGO_MEDIANO', 0.00, 'Multiplicador para pelo largo en perros medianos (sin incremento adicional)'),
    ('MULT_PELAJE_DOBLE_CAPA_MEDIANO', 0.25, 'Multiplicador adicional para doble capa en perros medianos (+25%)'),
    ('MULT_PELAJE_SEMI_LARGO_GRANDE', 0.25, 'Multiplicador adicional para pelo semi-largo en perros grandes (+25%)'),
    ('MULT_PELAJE_LARGO_GRANDE', 0.30, 'Multiplicador adicional para pelo largo en perros grandes (+30%)'),
    ('MULT_PELAJE_DOBLE_CAPA_GRANDE', 0.35, 'Multiplicador adicional para doble capa en perros grandes (+35%)')
) AS source (Clave, Valor, Descripcion)
ON target.Clave = source.Clave
WHEN MATCHED THEN
    UPDATE SET
        Valor = source.Valor,
        Descripcion = source.Descripcion,
        FechaActualizacion = GETUTCDATE()
WHEN NOT MATCHED THEN
    INSERT (Clave, Valor, Descripcion, FechaCreacion)
    VALUES (source.Clave, source.Valor, source.Descripcion, GETUTCDATE());

GO

-- Verificar las configuraciones actualizadas
SELECT * FROM ConfiguracionesPrecios WHERE Clave LIKE 'MULT_%' ORDER BY Clave;

PRINT '✓ Configuraciones de tamaño y pelaje actualizadas correctamente';
GO
