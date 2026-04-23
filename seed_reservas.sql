-- Seed de datos para módulo de reservas
-- Configuraciones de precio

-- Limpiar datos existentes si es necesario
DELETE FROM ConfiguracionesPrecios WHERE Clave LIKE 'MULT_%';

-- Insertar configuraciones de precio
INSERT INTO ConfiguracionesPrecios (Clave, Valor, Descripcion, FechaCreacion)
VALUES
('MULT_TAMANO_PEQUENO', 0.00, 'Multiplicador para tamaño pequeño (sin incremento)', GETDATE()),
('MULT_TAMANO_MEDIANO', 0.20, 'Multiplicador para tamaño mediano (20%)', GETDATE()),
('MULT_TAMANO_GRANDE', 0.50, 'Multiplicador para tamaño grande (50%)', GETDATE()),
('MULT_PELAJE_CORTO', 0.00, 'Multiplicador para pelaje corto (sin incremento)', GETDATE()),
('MULT_PELAJE_SEMI_LARGO', 0.15, 'Multiplicador para pelaje semi largo (15%)', GETDATE()),
('MULT_PELAJE_LARGO', 0.25, 'Multiplicador para pelaje largo (25%)', GETDATE()),
('MULT_PELAJE_DOBLE_CAPA', 0.45, 'Multiplicador para pelaje doble capa (45%)', GETDATE());

-- Insertar servicios
IF NOT EXISTS (SELECT 1 FROM Servicios WHERE Nombre = 'Baño Completo')
BEGIN
    INSERT INTO Servicios (Nombre, Descripcion, Precio, Activo, FechaCreacion)
    VALUES ('Baño Completo',
            'Servicio completo: Limpieza dental, limpieza de oídos, corte de uñas, champú desengrasante, champú hidratante, perfume y accesorios',
            50000.00,
            1,
            GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM Servicios WHERE Nombre = 'Corte de Uñas')
BEGIN
    INSERT INTO Servicios (Nombre, Descripcion, Precio, Activo, FechaCreacion)
    VALUES ('Corte de Uñas',
            'Corte profesional de uñas con técnica especializada',
            8000.00,
            1,
            GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM Servicios WHERE Nombre = 'Limpieza Dental')
BEGIN
    INSERT INTO Servicios (Nombre, Descripcion, Precio, Activo, FechaCreacion)
    VALUES ('Limpieza Dental',
            'Limpieza bucal completa y cuidado de encías',
            15000.00,
            1,
            GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM Servicios WHERE Nombre = 'Limpieza de Oídos')
BEGIN
    INSERT INTO Servicios (Nombre, Descripcion, Precio, Activo, FechaCreacion)
    VALUES ('Limpieza de Oídos',
            'Limpieza profunda de oídos para prevenir infecciones',
            10000.00,
            1,
            GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM Servicios WHERE Nombre = 'Corte de Pelaje')
BEGIN
    INSERT INTO Servicios (Nombre, Descripcion, Precio, Activo, FechaCreacion)
    VALUES ('Corte de Pelaje',
            'Corte de pelo según raza y preferencia del cliente',
            25000.00,
            1,
            GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM Servicios WHERE Nombre = 'Tratamiento Hidratante')
BEGIN
    INSERT INTO Servicios (Nombre, Descripcion, Precio, Activo, FechaCreacion)
    VALUES ('Tratamiento Hidratante',
            'Tratamiento especial para hidratar el pelaje y la piel',
            18000.00,
            1,
            GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM Servicios WHERE Nombre = 'Antiparasitario')
BEGIN
    INSERT INTO Servicios (Nombre, Descripcion, Precio, Activo, FechaCreacion)
    VALUES ('Antiparasitario',
            'Aplicación de tratamiento antipulgas y antigarrapatas',
            20000.00,
            1,
            GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM Servicios WHERE Nombre = 'Hidratación de Patas')
BEGIN
    INSERT INTO Servicios (Nombre, Descripcion, Precio, Activo, FechaCreacion)
    VALUES ('Hidratación de Patas',
            'Cuidado especial para almohadillas y patas',
            12000.00,
            1,
            GETDATE());
END

-- Verificar datos insertados
SELECT * FROM ConfiguracionesPrecios;
SELECT * FROM Servicios;
