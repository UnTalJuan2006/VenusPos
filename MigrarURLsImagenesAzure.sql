-- ═══════════════════════════════════════════════════════════════════════════
-- Script para migrar URLs de imágenes locales a Azure Blob Storage
-- ═══════════════════════════════════════════════════════════════════════════
--
-- IMPORTANTE: Ejecuta este script DESPUÉS de haber subido las imágenes a Azure
--
-- Este script convierte URLs como:
--   /img/empleados/12345678-1234-1234-1234-123456789abc.jpg
-- En URLs de Azure como:
--   https://TU_NOMBRE_CUENTA.blob.core.windows.net/empleados/12345678-1234-1234-1234-123456789abc.jpg
--
-- ═══════════════════════════════════════════════════════════════════════════

USE VenusPosDB;
GO

-- ═══════════════════════════════════════════════════════════════════════════
-- CONFIGURACIÓN: Cambia esto por el nombre de tu cuenta de Azure Storage
-- ═══════════════════════════════════════════════════════════════════════════
DECLARE @AzureStorageAccount NVARCHAR(100) = 'TU_NOMBRE_CUENTA'; -- ⚠️ CAMBIA ESTO
DECLARE @BaseURL NVARCHAR(200) = 'https://' + @AzureStorageAccount + '.blob.core.windows.net/';

-- ═══════════════════════════════════════════════════════════════════════════
-- Verificar configuración
-- ═══════════════════════════════════════════════════════════════════════════
IF @AzureStorageAccount = 'TU_NOMBRE_CUENTA'
BEGIN
    PRINT '⚠️  ERROR: Debes configurar el nombre de tu cuenta de Azure Storage';
    PRINT '⚠️  Edita la variable @AzureStorageAccount en la línea 16 de este script';
    PRINT '';
    PRINT 'Ejemplo:';
    PRINT '  DECLARE @AzureStorageAccount NVARCHAR(100) = ''venusposimages'';';
    RETURN;
END

PRINT '';
PRINT '═══════════════════════════════════════════════════════════════════════════';
PRINT 'Iniciando migración de URLs de imágenes a Azure Blob Storage';
PRINT '═══════════════════════════════════════════════════════════════════════════';
PRINT '';
PRINT 'Cuenta de Azure Storage: ' + @AzureStorageAccount;
PRINT 'URL base: ' + @BaseURL;
PRINT '';

-- ═══════════════════════════════════════════════════════════════════════════
-- Mostrar imágenes que se van a migrar
-- ═══════════════════════════════════════════════════════════════════════════
PRINT '── EMPLEADOS CON IMÁGENES LOCALES ──────────────────────────────────────────';
SELECT
    Id,
    Nombre,
    Imagen AS 'URL Actual',
    @BaseURL + 'empleados/' + RIGHT(Imagen, CHARINDEX('/', REVERSE(Imagen)) - 1) AS 'Nueva URL'
FROM Empleados
WHERE Imagen IS NOT NULL
  AND Imagen LIKE '%/img/empleados/%';

DECLARE @TotalEmpleados INT = (
    SELECT COUNT(*)
    FROM Empleados
    WHERE Imagen IS NOT NULL AND Imagen LIKE '%/img/empleados/%'
);

PRINT '';
PRINT 'Total de empleados a migrar: ' + CAST(@TotalEmpleados AS VARCHAR(10));
PRINT '';

PRINT '── MASCOTAS CON IMÁGENES LOCALES ───────────────────────────────────────────';
SELECT
    Id,
    Nombre,
    Imagen AS 'URL Actual',
    @BaseURL + 'mascotas/' + RIGHT(Imagen, CHARINDEX('/', REVERSE(Imagen)) - 1) AS 'Nueva URL'
FROM Mascotas
WHERE Imagen IS NOT NULL
  AND Imagen LIKE '%/img/mascotas/%';

DECLARE @TotalMascotas INT = (
    SELECT COUNT(*)
    FROM Mascotas
    WHERE Imagen IS NOT NULL AND Imagen LIKE '%/img/mascotas/%'
);

PRINT '';
PRINT 'Total de mascotas a migrar: ' + CAST(@TotalMascotas AS VARCHAR(10));
PRINT '';

-- ═══════════════════════════════════════════════════════════════════════════
-- Confirmar antes de proceder
-- ═══════════════════════════════════════════════════════════════════════════
PRINT '═══════════════════════════════════════════════════════════════════════════';
PRINT '⚠️  ADVERTENCIA: Esta operación actualizará las URLs en la base de datos';
PRINT '⚠️  Asegúrate de haber subido las imágenes a Azure antes de continuar';
PRINT '';
PRINT 'Si estás seguro de continuar, ejecuta la sección de MIGRACIÓN más abajo.';
PRINT '═══════════════════════════════════════════════════════════════════════════';
PRINT '';

-- ═══════════════════════════════════════════════════════════════════════════
-- DESCOMENTAR ESTA SECCIÓN PARA EJECUTAR LA MIGRACIÓN
-- ═══════════════════════════════════════════════════════════════════════════

/*
BEGIN TRANSACTION;

BEGIN TRY
    -- Actualizar URLs de empleados
    UPDATE Empleados
    SET Imagen = @BaseURL + 'empleados/' + RIGHT(Imagen, CHARINDEX('/', REVERSE(Imagen)) - 1)
    WHERE Imagen IS NOT NULL
      AND Imagen LIKE '%/img/empleados/%';

    DECLARE @EmpleadosActualizados INT = @@ROWCOUNT;

    -- Actualizar URLs de mascotas
    UPDATE Mascotas
    SET Imagen = @BaseURL + 'mascotas/' + RIGHT(Imagen, CHARINDEX('/', REVERSE(Imagen)) - 1)
    WHERE Imagen IS NOT NULL
      AND Imagen LIKE '%/img/mascotas/%';

    DECLARE @MascotasActualizadas INT = @@ROWCOUNT;

    -- Si todo salió bien, confirmar cambios
    COMMIT TRANSACTION;

    PRINT '';
    PRINT '═══════════════════════════════════════════════════════════════════════════';
    PRINT '✓ MIGRACIÓN COMPLETADA EXITOSAMENTE';
    PRINT '═══════════════════════════════════════════════════════════════════════════';
    PRINT '';
    PRINT 'Empleados actualizados: ' + CAST(@EmpleadosActualizados AS VARCHAR(10));
    PRINT 'Mascotas actualizadas: ' + CAST(@MascotasActualizadas AS VARCHAR(10));
    PRINT '';
    PRINT '✓ Las URLs ahora apuntan a Azure Blob Storage';
    PRINT '';

END TRY
BEGIN CATCH
    -- Si hubo un error, revertir cambios
    ROLLBACK TRANSACTION;

    PRINT '';
    PRINT '═══════════════════════════════════════════════════════════════════════════';
    PRINT '❌ ERROR EN LA MIGRACIÓN';
    PRINT '═══════════════════════════════════════════════════════════════════════════';
    PRINT 'Error: ' + ERROR_MESSAGE();
    PRINT 'Línea: ' + CAST(ERROR_LINE() AS VARCHAR(10));
    PRINT '';
    PRINT 'No se realizaron cambios en la base de datos.';
    PRINT '';

END CATCH;
*/

-- ═══════════════════════════════════════════════════════════════════════════
-- Script para REVERTIR la migración (solo si algo salió mal)
-- ═══════════════════════════════════════════════════════════════════════════

/*
BEGIN TRANSACTION;

-- Revertir URLs de empleados
UPDATE Empleados
SET Imagen = '/img/empleados/' + RIGHT(Imagen, CHARINDEX('/', REVERSE(Imagen)) - 1)
WHERE Imagen IS NOT NULL
  AND Imagen LIKE 'https://%blob.core.windows.net/empleados/%';

-- Revertir URLs de mascotas
UPDATE Mascotas
SET Imagen = '/img/mascotas/' + RIGHT(Imagen, CHARINDEX('/', REVERSE(Imagen)) - 1)
WHERE Imagen IS NOT NULL
  AND Imagen LIKE 'https://%blob.core.windows.net/mascotas/%';

COMMIT TRANSACTION;

PRINT '✓ URLs revertidas a formato local';
*/

GO
