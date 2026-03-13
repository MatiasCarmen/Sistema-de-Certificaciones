-- =====================================================================
-- SCRIPT DE MIGRACIÓN DE PROCEDIMIENTOS ALMACENADOS
-- Proyecto: Inkillay Certificados
-- Fecha: 2024
-- Estándar: Corporativo SQL Server 2019
-- =====================================================================

-- ⚠️ IMPORTANTE: Ejecutar este script en AMBIENTE DE DESARROLLO primero
-- ⚠️ Realizar backup completo antes de ejecutar en PRODUCCIÓN

USE [InkillayCertificados];
GO

-- =====================================================================
-- SECCIÓN 1: PROCEDIMIENTOS DE USUARIOS
-- =====================================================================

-- USP_Usuarios_Registrar: Agregar campos de auditoría
ALTER PROCEDURE [dbo].[USP_Usuarios_Registrar]
    @Nombre NVARCHAR(300),
    @Correo NVARCHAR(300),
    @Clave NVARCHAR(MAX),
    @IdRol INT,
    @UsuarioRegistro VARCHAR(40) = 'Sistema'  -- ✅ NUEVO PARÁMETRO
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO Usuarios (
        Nombre, 
        Correo, 
        Clave, 
        IdRol, 
        Estado,  -- ✅ CHAR(1) en lugar de BIT
        UsuarioRegistro,  -- ✅ NUEVO
        UsuarioModifica   -- ✅ NUEVO
    )
    VALUES (
        @Nombre, 
        @Correo, 
        @Clave, 
        @IdRol, 
        'A',  -- ✅ 'A' en lugar de 1
        @UsuarioRegistro,
        @UsuarioRegistro
    );
    -- FechaRegistro se asigna automáticamente por DEFAULT GETDATE()
END;
GO

-- USP_Usuarios_ValidarAcceso: Cambiar filtro de Estado
ALTER PROCEDURE [dbo].[USP_Usuarios_ValidarAcceso]
    @Correo NVARCHAR(300)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        IdUsuario, 
        Nombre, 
        Correo, 
        Clave, 
        IdRol, 
        Estado,
        UsuarioRegistro,
        FechaRegistro
    FROM Usuarios
    WHERE Correo = @Correo 
      AND Estado = 'A';  -- ✅ 'A' en lugar de 1
END;
GO

-- USP_Usuarios_ObtenerPorCorreo: Cambiar filtro de Estado
ALTER PROCEDURE [dbo].[USP_Usuarios_ObtenerPorCorreo]
    @Correo NVARCHAR(300)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        IdUsuario, 
        Nombre, 
        Correo, 
        Clave, 
        IdRol, 
        Estado,
        UsuarioRegistro,
        FechaRegistro,
        UsuarioModifica,
        FechaModifica
    FROM Usuarios
    WHERE Correo = @Correo 
      AND Estado = 'A';  -- ✅ 'A' en lugar de 1
END;
GO

-- USP_Usuarios_CambiarEstado: Ahora recibe CHAR en lugar de BIT
ALTER PROCEDURE [dbo].[USP_Usuarios_CambiarEstado]
    @IdUsuario INT,
    @Estado CHAR(1),  -- ✅ CHAR(1) en lugar de BIT
    @UsuarioModifica VARCHAR(40) = 'Sistema'  -- ✅ NUEVO
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Usuarios
    SET Estado = @Estado,
        UsuarioModifica = @UsuarioModifica,  -- ✅ NUEVO
        FechaModifica = GETDATE()            -- ✅ NUEVO
    WHERE IdUsuario = @IdUsuario;
END;
GO

-- =====================================================================
-- SECCIÓN 2: PROCEDIMIENTOS DE CURSOS
-- =====================================================================

-- USP_Cursos_ListarActivos: Cambiar filtro de Estado
ALTER PROCEDURE [dbo].[USP_Cursos_ListarActivos]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        IdCurso, 
        Nombre, 
        Sumilla, 
        FechaInicio, 
        FechaFin, 
        IdProfesor, 
        Costo, 
        Estado,
        UsuarioRegistro,
        FechaRegistro
    FROM Cursos
    WHERE Estado = 'A'  -- ✅ 'A' en lugar de 1
    ORDER BY FechaInicio DESC;
END;
GO

-- USP_Cursos_Insertar: Agregar campos de auditoría (si existe)
-- Si este SP no existe, es solo un ejemplo de cómo debería crearse
CREATE OR ALTER PROCEDURE [dbo].[USP_Cursos_Insertar]
    @Nombre NVARCHAR(200),
    @Sumilla NVARCHAR(MAX) = NULL,
    @FechaInicio DATE = NULL,
    @FechaFin DATE = NULL,
    @Costo DECIMAL(10,2) = 0,
    @IdProfesor INT = NULL,
    @UsuarioRegistro VARCHAR(40) = 'Sistema'  -- ✅ NUEVO
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO Cursos (
        Nombre, 
        Sumilla, 
        FechaInicio, 
        FechaFin, 
        Costo, 
        IdProfesor, 
        Estado,
        UsuarioRegistro,
        UsuarioModifica
    )
    VALUES (
        @Nombre, 
        @Sumilla, 
        @FechaInicio, 
        @FechaFin, 
        @Costo, 
        @IdProfesor, 
        'A',  -- ✅ Activo por defecto
        @UsuarioRegistro,
        @UsuarioRegistro
    );
    
    SELECT SCOPE_IDENTITY() AS IdCurso;
END;
GO

-- =====================================================================
-- SECCIÓN 3: PROCEDIMIENTOS DE MATRICULAS
-- =====================================================================

-- USP_Matriculas_Registrar: Agregar nuevos campos EstadoMatricula y Modalidad
ALTER PROCEDURE [dbo].[USP_Matriculas_Registrar]
    @IdAlumno INT,
    @IdCurso INT,
    @UsuarioRegistro VARCHAR(40) = 'Sistema'  -- ✅ NUEVO
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Verificar si ya existe la matrícula
    IF EXISTS (SELECT 1 FROM Matriculas WHERE IdAlumno = @IdAlumno AND IdCurso = @IdCurso)
    BEGIN
        RAISERROR('El alumno ya está matriculado en este curso', 16, 1);
        RETURN;
    END
    
    INSERT INTO Matriculas (
        IdAlumno, 
        IdCurso, 
        Aprobado,
        EstadoMatricula,  -- ✅ NUEVO (1 = Registrada)
        Modalidad,        -- ✅ NUEVO (P = Presencial)
        UsuarioRegistro,
        UsuarioModifica
    )
    VALUES (
        @IdAlumno, 
        @IdCurso, 
        0,    -- No aprobado por defecto
        1,    -- ✅ Estado: Registrada
        'P',  -- ✅ Modalidad: Presencial por defecto
        @UsuarioRegistro,
        @UsuarioRegistro
    );
    -- FechaRegistro (antes FechaMatricula) se asigna automáticamente
END;
GO

-- USP_Matriculas_ListarAlumnosPorCurso: Incluir nuevos campos
ALTER PROCEDURE [dbo].[USP_Matriculas_ListarAlumnosPorCurso]
    @IdCurso INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        m.IdMatricula,
        m.IdAlumno,
        m.IdCurso,
        m.Aprobado,
        m.EstadoMatricula,  -- ✅ NUEVO
        m.Modalidad,        -- ✅ NUEVO
        m.FechaRegistro AS FechaMatricula,  -- ✅ Alias para compatibilidad
        u.Nombre AS NombreAlumno,
        c.Nombre AS NombreCurso,
        c.Costo AS CostoCurso,
        c.Sumilla,
        ISNULL((SELECT SUM(Monto) FROM Pagos WHERE IdMatricula = m.IdMatricula), 0) AS TotalPagado
    FROM Matriculas m
    INNER JOIN Usuarios u ON m.IdAlumno = u.IdUsuario
    INNER JOIN Cursos c ON m.IdCurso = c.IdCurso
    WHERE m.IdCurso = @IdCurso
    ORDER BY m.FechaRegistro DESC;
END;
GO

-- =====================================================================
-- SECCIÓN 4: PROCEDIMIENTOS DE PAGOS
-- =====================================================================

-- USP_Pagos_Registrar: Agregar FormaPago, TipoPago y auditoría
ALTER PROCEDURE [dbo].[USP_Pagos_Registrar]
    @IdMatricula INT,
    @Monto DECIMAL(10,2),
    @Referencia NVARCHAR(200),
    @FormaPago VARCHAR(20) = 'Efectivo',      -- ✅ NUEVO
    @TipoPago VARCHAR(20) = 'Contado',        -- ✅ NUEVO
    @UsuarioRegistro VARCHAR(40) = 'Sistema'  -- ✅ NUEVO
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validar que la matrícula existe
    IF NOT EXISTS (SELECT 1 FROM Matriculas WHERE IdMatricula = @IdMatricula)
    BEGIN
        RAISERROR('La matrícula no existe', 16, 1);
        RETURN;
    END
    
    INSERT INTO Pagos (
        IdMatricula, 
        Monto, 
        Referencia,
        FormaPago,       -- ✅ NUEVO
        TipoPago,        -- ✅ NUEVO
        UsuarioRegistro,
        UsuarioModifica
    )
    VALUES (
        @IdMatricula, 
        @Monto, 
        @Referencia,
        @FormaPago,
        @TipoPago,
        @UsuarioRegistro,
        @UsuarioRegistro
    );
    -- FechaPago y FechaRegistro se asignan automáticamente por DEFAULT GETDATE()
    
    SELECT 1 AS Resultado;  -- Devuelve 1 si fue exitoso
END;
GO

-- =====================================================================
-- SECCIÓN 5: PROCEDIMIENTOS DE PLANTILLAS
-- =====================================================================

-- USP_Plantillas_ListarTodos: Incluir FontFamily y cambiar filtro de Estado
ALTER PROCEDURE [dbo].[USP_Plantillas_ListarTodos]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        IdPlantilla, 
        Nombre, 
        RutaImagen, 
        EjeX, 
        EjeY, 
        FontSize, 
        FontColor,
        FontFamily,  -- ✅ NUEVO
        Estado,
        UsuarioRegistro,
        FechaRegistro
    FROM Plantillas
    -- Devuelve todas, tanto activas como inactivas
    ORDER BY IdPlantilla DESC;
END;
GO

-- USP_Plantillas_Insertar: Agregar FontFamily y auditoría
ALTER PROCEDURE [dbo].[USP_Plantillas_Insertar]
    @Nombre NVARCHAR(200),
    @RutaImagen NVARCHAR(510),
    @FontFamily NVARCHAR(100) = 'Arial',      -- ✅ NUEVO
    @UsuarioRegistro VARCHAR(40) = 'Sistema'  -- ✅ NUEVO
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO Plantillas (
        Nombre, 
        RutaImagen, 
        EjeX, 
        EjeY, 
        FontSize, 
        FontColor,
        FontFamily,  -- ✅ NUEVO
        Estado,
        UsuarioRegistro,
        UsuarioModifica
    )
    VALUES (
        @Nombre, 
        @RutaImagen, 
        50,    -- Default
        50,    -- Default
        60,    -- Default
        '#000000',  -- Default negro
        @FontFamily,
        'A',   -- ✅ Activa por defecto
        @UsuarioRegistro,
        @UsuarioRegistro
    );
    
    SELECT SCOPE_IDENTITY() AS IdPlantilla;
END;
GO

-- USP_Plantillas_ActualizarCoordenadas: Agregar auditoría
ALTER PROCEDURE [dbo].[USP_Plantillas_ActualizarCoordenadas]
    @IdPlantilla INT,
    @EjeX INT,
    @EjeY INT,
    @UsuarioModifica VARCHAR(40) = 'Sistema'  -- ✅ NUEVO
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Plantillas
    SET EjeX = @EjeX,
        EjeY = @EjeY,
        UsuarioModifica = @UsuarioModifica,  -- ✅ NUEVO
        FechaModifica = GETDATE()            -- ✅ NUEVO
    WHERE IdPlantilla = @IdPlantilla;
END;
GO

-- USP_Plantillas_ActualizarDiseno: Agregar auditoría
ALTER PROCEDURE [dbo].[USP_Plantillas_ActualizarDiseno]
    @IdPlantilla INT,
    @EjeX INT,
    @EjeY INT,
    @FontSize INT,
    @FontColor NVARCHAR(20),
    @UsuarioModifica VARCHAR(40) = 'Sistema'  -- ✅ NUEVO
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Plantillas
    SET EjeX = @EjeX,
        EjeY = @EjeY,
        FontSize = @FontSize,
        FontColor = @FontColor,
        UsuarioModifica = @UsuarioModifica,  -- ✅ NUEVO
        FechaModifica = GETDATE()            -- ✅ NUEVO
    WHERE IdPlantilla = @IdPlantilla;
END;
GO

-- USP_Plantillas_CambiarEstado: Ahora alterna entre 'A' e 'I'
ALTER PROCEDURE [dbo].[USP_Plantillas_CambiarEstado]
    @IdPlantilla INT,
    @UsuarioModifica VARCHAR(40) = 'Sistema'  -- ✅ NUEVO
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Plantillas
    SET Estado = CASE 
                     WHEN Estado = 'A' THEN 'I'  -- ✅ Si está activo, desactivar
                     ELSE 'A'                    -- ✅ Si está inactivo, activar
                 END,
        UsuarioModifica = @UsuarioModifica,  -- ✅ NUEVO
        FechaModifica = GETDATE()            -- ✅ NUEVO
    WHERE IdPlantilla = @IdPlantilla;
END;
GO

-- =====================================================================
-- SECCIÓN 6: PROCEDIMIENTOS DE SEGURIDAD
-- =====================================================================

-- USP_Seg_Modulo_Listar: Cambiar filtro de Estado
ALTER PROCEDURE [dbo].[USP_Seg_Modulo_Listar]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        IdModulo, 
        Nombre, 
        Icono, 
        Estado,
        UsuarioRegistro,
        FechaRegistro
    FROM Seg_Modulo
    WHERE Estado = 'A'  -- ✅ Solo módulos activos
    ORDER BY IdModulo;
END;
GO

-- =====================================================================
-- VERIFICACIÓN POST-MIGRACIÓN
-- =====================================================================

-- Verificar que los cambios se aplicaron correctamente
PRINT '========================================';
PRINT 'VERIFICACIÓN DE PROCEDIMIENTOS';
PRINT '========================================';

-- Listar todos los procedimientos almacenados modificados
SELECT 
    OBJECT_NAME(object_id) AS NombreSP,
    create_date AS FechaCreacion,
    modify_date AS FechaModificacion
FROM sys.procedures
WHERE OBJECT_NAME(object_id) LIKE 'USP_%'
ORDER BY modify_date DESC;

PRINT 'Migración de procedimientos almacenados completada.';
PRINT 'Recuerda probar cada SP antes de continuar.';
GO
