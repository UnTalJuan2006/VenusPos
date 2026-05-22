-- =====================================================
-- SCRIPT PARA LIMPIAR TODAS LAS TABLAS DEL SISTEMA
-- =====================================================
-- Este script elimina todos los datos de todas las tablas
-- y resetea los contadores de identidad
-- =====================================================

BEGIN TRANSACTION;

BEGIN TRY
    -- Deshabilitar restricciones de claves foráneas temporalmente
    EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';

    -- =====================================================
    -- ELIMINAR DATOS DE TODAS LAS TABLAS
    -- =====================================================

    -- Tablas dependientes de nivel superior
    DELETE FROM [dbo].[Notificaciones];
    DELETE FROM [dbo].[Historiales];
    DELETE FROM [dbo].[VentaDetalles];
    DELETE FROM [dbo].[Ventas];
    DELETE FROM [dbo].[MovimientosCaja];

    -- Tablas de Caja
    DELETE FROM [dbo].[Caja];

    -- Tablas relacionales de Reserva
    DELETE FROM [dbo].[ReservaServicios];
    DELETE FROM [dbo].[ReservaMascotas];

    -- Tablas de Reservas
    DELETE FROM [dbo].[Reservas];

    -- Tablas relacionales de Mascota y Empleado
    DELETE FROM [dbo].[MascotaServicios];
    DELETE FROM [dbo].[EmpleadoServicios];

    -- Tablas principales
    DELETE FROM [dbo].[Mascotas];
    DELETE FROM [dbo].[Empleados];
    DELETE FROM [dbo].[Clientes];
    DELETE FROM [dbo].[Servicios];

    -- Tabla de configuración
    DELETE FROM [dbo].[ConfiguracionesPrecios];

    -- =====================================================
    -- RESETEAR CONTADORES DE IDENTIDAD (IDENTITY)
    -- =====================================================

    -- Tablas con IDENTITY
    DBCC CHECKIDENT ('[dbo].[Notificaciones]', RESEED, 0);
    DBCC CHECKIDENT ('[dbo].[Historiales]', RESEED, 0);
    DBCC CHECKIDENT ('[dbo].[VentaDetalles]', RESEED, 0);
    DBCC CHECKIDENT ('[dbo].[Ventas]', RESEED, 0);
    DBCC CHECKIDENT ('[dbo].[MovimientosCaja]', RESEED, 0);
    DBCC CHECKIDENT ('[dbo].[Caja]', RESEED, 0);
    DBCC CHECKIDENT ('[dbo].[Reservas]', RESEED, 0);
    DBCC CHECKIDENT ('[dbo].[Mascotas]', RESEED, 0);
    DBCC CHECKIDENT ('[dbo].[Empleados]', RESEED, 0);
    DBCC CHECKIDENT ('[dbo].[Clientes]', RESEED, 0);
    DBCC CHECKIDENT ('[dbo].[Servicios]', RESEED, 0);
    DBCC CHECKIDENT ('[dbo].[ConfiguracionesPrecios]', RESEED, 0);

    -- Rehabilitar restricciones de claves foráneas
    EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';

    -- Si todo salió bien, confirmar la transacción
    COMMIT TRANSACTION;

    PRINT 'PROCESO COMPLETADO EXITOSAMENTE';
    PRINT '================================';
    PRINT 'Todas las tablas han sido limpiadas';
    PRINT 'Los contadores de identidad han sido reseteados';

END TRY
BEGIN CATCH
    -- Si hubo algún error, revertir todos los cambios
    ROLLBACK TRANSACTION;

    PRINT 'ERROR AL LIMPIAR LAS TABLAS';
    PRINT '===========================';
    PRINT 'Error: ' + ERROR_MESSAGE();
    PRINT 'Línea: ' + CAST(ERROR_LINE() AS VARCHAR(10));
    PRINT 'Procedimiento: ' + ISNULL(ERROR_PROCEDURE(), 'Script');

END CATCH;

-- Verificar el estado de las tablas
SELECT
    t.name AS 'Tabla',
    SUM(p.rows) AS 'Registros'
FROM
    sys.tables t
INNER JOIN
    sys.partitions p ON t.object_id = p.object_id
WHERE
    t.is_ms_shipped = 0
    AND p.index_id IN (0,1)
    AND t.name IN (
        'Notificaciones', 'Historiales', 'VentaDetalles', 'Ventas',
        'MovimientosCaja', 'Caja', 'ReservaServicios', 'ReservaMascotas',
        'Reservas', 'MascotaServicios', 'EmpleadoServicios', 'Mascotas',
        'Empleados', 'Clientes', 'Servicios', 'ConfiguracionesPrecios'
    )
GROUP BY
    t.name
ORDER BY
    t.name;
