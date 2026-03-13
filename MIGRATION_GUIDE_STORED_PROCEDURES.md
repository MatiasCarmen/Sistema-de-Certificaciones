# 📋 GUÍA DE MIGRACIÓN - PROCEDIMIENTOS ALMACENADOS

## ⚠️ CAMBIOS CRÍTICOS EN BASE DE DATOS

### 1. Campo `Estado` migrado de BIT a CHAR(1)
**Antes:** `Estado BIT` (0 = Inactivo, 1 = Activo)  
**Ahora:** `Estado CHAR(1)` ('I' = Inactivo, 'A' = Activo)

#### ✅ Actualizar TODOS los SPs que usan Estado:
```sql
-- ❌ ANTES (incorrecto):
WHERE Estado = 1

-- ✅ AHORA (correcto):
WHERE Estado = 'A'

-- ❌ ANTES (incorrecto):
INSERT INTO Usuarios (..., Estado) VALUES (..., 1)

-- ✅ AHORA (correcto):
INSERT INTO Usuarios (..., Estado) VALUES (..., 'A')

-- ❌ ANTES (incorrecto):
UPDATE Usuarios SET Estado = @Estado  -- @Estado es BIT

-- ✅ AHORA (correcto):
UPDATE Usuarios SET Estado = @Estado  -- @Estado es CHAR(1)
```

### 2. Campos de Auditoría (NUEVOS)
**Tablas afectadas:** Usuarios, Cursos, Matriculas, Plantillas, PlantillaDetalle, Pagos, Roles, Seg_Modulo, Seg_Opcion

**Campos obligatorios:**
- `UsuarioRegistro VARCHAR(40)` - Usuario que creó el registro
- `FechaRegistro DATETIME` - Fecha de creación (DEFAULT GETDATE())
- `UsuarioModifica VARCHAR(40)` - Usuario que modificó el registro
- `FechaModifica DATETIME` - Fecha de modificación

#### ✅ Actualizar SPs de INSERT:
```sql
-- ❌ ANTES (incorrecto):
CREATE PROCEDURE USP_Usuarios_Registrar
    @Nombre NVARCHAR(300),
    @Correo NVARCHAR(300),
    @Clave NVARCHAR(MAX),
    @IdRol INT
AS
BEGIN
    INSERT INTO Usuarios (Nombre, Correo, Clave, IdRol, Estado)
    VALUES (@Nombre, @Correo, @Clave, @IdRol, 1)
END

-- ✅ AHORA (correcto):
CREATE PROCEDURE USP_Usuarios_Registrar
    @Nombre NVARCHAR(300),
    @Correo NVARCHAR(300),
    @Clave NVARCHAR(MAX),
    @IdRol INT,
    @UsuarioRegistro VARCHAR(40) = 'Sistema'  -- NUEVO
AS
BEGIN
    INSERT INTO Usuarios (Nombre, Correo, Clave, IdRol, Estado, UsuarioRegistro, UsuarioModifica)
    VALUES (@Nombre, @Correo, @Clave, @IdRol, 'A', @UsuarioRegistro, @UsuarioRegistro)
    -- Nota: FechaRegistro se asigna automáticamente por DEFAULT GETDATE()
END
```

#### ✅ Actualizar SPs de UPDATE:
```sql
-- ❌ ANTES (incorrecto):
CREATE PROCEDURE USP_Usuarios_Actualizar
    @IdUsuario INT,
    @Nombre NVARCHAR(300),
    @IdRol INT
AS
BEGIN
    UPDATE Usuarios 
    SET Nombre = @Nombre, IdRol = @IdRol
    WHERE IdUsuario = @IdUsuario
END

-- ✅ AHORA (correcto):
CREATE PROCEDURE USP_Usuarios_Actualizar
    @IdUsuario INT,
    @Nombre NVARCHAR(300),
    @IdRol INT,
    @UsuarioModifica VARCHAR(40) = 'Sistema'  -- NUEVO
AS
BEGIN
    UPDATE Usuarios 
    SET Nombre = @Nombre, 
        IdRol = @IdRol,
        UsuarioModifica = @UsuarioModifica,  -- NUEVO
        FechaModifica = GETDATE()             -- NUEVO
    WHERE IdUsuario = @IdUsuario
END
```

### 3. Cambios en Tabla Matriculas

#### Campo renombrado:
- **ANTES:** `FechaMatricula DATETIME`
- **AHORA:** `FechaRegistro DATETIME` (campo de auditoría)

#### Nuevos campos de negocio:
```sql
EstadoMatricula INT DEFAULT 1 CHECK (EstadoMatricula BETWEEN 1 AND 6)
    -- 1: Registrada, 2: En curso, 3: Completada, 4: Aprobada, 5: Reprobada, 6: Cancelada

Modalidad CHAR(1) DEFAULT 'P' CHECK (Modalidad IN ('P', 'R', 'H'))
    -- P: Presencial, R: Remoto, H: Híbrido
```

#### ✅ Actualizar SPs de Matriculas:
```sql
-- ❌ ANTES (incorrecto):
SELECT IdMatricula, IdAlumno, IdCurso, FechaMatricula, Aprobado
FROM Matriculas

-- ✅ AHORA (correcto):
SELECT IdMatricula, IdAlumno, IdCurso, FechaRegistro AS FechaMatricula, 
       Aprobado, EstadoMatricula, Modalidad
FROM Matriculas

-- ❌ ANTES (incorrecto):
INSERT INTO Matriculas (IdAlumno, IdCurso)
VALUES (@IdAlumno, @IdCurso)

-- ✅ AHORA (correcto):
INSERT INTO Matriculas (IdAlumno, IdCurso, EstadoMatricula, Modalidad, UsuarioRegistro, UsuarioModifica)
VALUES (@IdAlumno, @IdCurso, 1, 'P', @UsuarioRegistro, @UsuarioRegistro)
```

### 4. Cambios en Tabla Pagos

#### Nuevos campos:
```sql
FormaPago VARCHAR(20) NULL  -- Efectivo, Tarjeta, Transferencia, Yape, Plin
TipoPago VARCHAR(20) NULL   -- Contado, Fraccionado, Cuota
```

#### ✅ Actualizar SP de Pagos:
```sql
-- ❌ ANTES (incorrecto):
CREATE PROCEDURE USP_Pagos_Registrar
    @IdMatricula INT,
    @Monto DECIMAL(10,2),
    @Referencia NVARCHAR(200)
AS
BEGIN
    INSERT INTO Pagos (IdMatricula, Monto, Referencia)
    VALUES (@IdMatricula, @Monto, @Referencia)
END

-- ✅ AHORA (correcto):
CREATE PROCEDURE USP_Pagos_Registrar
    @IdMatricula INT,
    @Monto DECIMAL(10,2),
    @Referencia NVARCHAR(200),
    @FormaPago VARCHAR(20) = 'Efectivo',      -- NUEVO
    @TipoPago VARCHAR(20) = 'Contado',        -- NUEVO
    @UsuarioRegistro VARCHAR(40) = 'Sistema'  -- NUEVO
AS
BEGIN
    INSERT INTO Pagos (IdMatricula, Monto, Referencia, FormaPago, TipoPago, UsuarioRegistro, UsuarioModifica)
    VALUES (@IdMatricula, @Monto, @Referencia, @FormaPago, @TipoPago, @UsuarioRegistro, @UsuarioRegistro)
END
```

### 5. Cambios en Tabla Plantillas

#### Nuevo campo:
```sql
FontFamily NVARCHAR(100) DEFAULT 'Arial'
```

#### ✅ Actualizar SPs de Plantillas:
```sql
-- Al insertar o actualizar, incluir FontFamily:
INSERT INTO Plantillas (Nombre, RutaImagen, FontFamily, Estado, UsuarioRegistro, UsuarioModifica)
VALUES (@Nombre, @RutaImagen, 'Arial', 'A', @UsuarioRegistro, @UsuarioRegistro)
```

---

## 📝 LISTA DE PROCEDIMIENTOS QUE DEBEN ACTUALIZARSE

### Usuarios:
- ✅ `USP_Usuarios_Registrar` - Agregar @UsuarioRegistro, cambiar Estado a 'A'
- ✅ `USP_Usuarios_ValidarAcceso` - Cambiar WHERE Estado = 1 por Estado = 'A'
- ✅ `USP_Usuarios_ObtenerPorCorreo` - Cambiar WHERE Estado = 1 por Estado = 'A'
- ✅ `USP_Usuarios_ObtenerPorId` - Sin cambios en WHERE, pero devuelve Estado CHAR
- ✅ `USP_Usuarios_ListarTodos` - Devuelve Estado CHAR
- ✅ `USP_Usuarios_CorreoExiste` - Sin cambios
- ✅ `USP_Usuarios_CambiarEstado` - @Estado ahora es CHAR(1), no BIT

### Cursos:
- ✅ `USP_Cursos_ListarTodos` - Devuelve Estado CHAR
- ✅ `USP_Cursos_ListarActivos` - Cambiar WHERE Estado = 1 por Estado = 'A'
- ✅ `USP_Cursos_ObtenerPorId` - Devuelve Estado CHAR
- ✅ `USP_Cursos_Insertar` (si existe) - Agregar auditoría, Estado = 'A'
- ✅ `USP_Cursos_Actualizar` (si existe) - Agregar @UsuarioModifica, @FechaModifica

### Matriculas:
- ✅ `USP_Matriculas_ListarAlumnosPorCurso` - Incluir EstadoMatricula, Modalidad, FechaRegistro
- ✅ `USP_Matriculas_Registrar` - Agregar EstadoMatricula=1, Modalidad='P', @UsuarioRegistro
- ✅ `USP_Alumnos_ListarMisCursos` - Incluir EstadoMatricula, Modalidad

### Pagos:
- ✅ `USP_Pagos_Registrar` - Agregar FormaPago, TipoPago, @UsuarioRegistro

### Plantillas:
- ✅ `USP_Plantillas_ListarTodos` - Cambiar WHERE Estado = 1 por Estado = 'A', incluir FontFamily
- ✅ `USP_Plantillas_Insertar` - Agregar FontFamily, @UsuarioRegistro, Estado = 'A'
- ✅ `USP_Plantillas_ActualizarCoordenadas` - Agregar @UsuarioModifica
- ✅ `USP_Plantillas_ActualizarDiseno` - Agregar @UsuarioModifica
- ✅ `USP_Plantillas_CambiarEstado` - @Estado ahora es CHAR(1)
- ✅ `USP_PlantillaDetalle_Listar` - Sin cambios de lógica
- ✅ `USP_Plantillas_GuardarDisenoCompleto` - Agregar @UsuarioModifica

### Seguridad:
- ✅ `USP_Seg_Modulo_Listar` - Cambiar WHERE Estado = 1 por Estado = 'A'

---

## 🔧 SCRIPT DE MIGRACIÓN RÁPIDA (Ejecutar en SQL Server)

```sql
-- Actualizar todos los WHERE Estado = 1 a Estado = 'A'
-- Nota: Esto es solo un ejemplo, cada SP debe revisarse manualmente

-- Ejemplo para USP_Usuarios_ValidarAcceso:
ALTER PROCEDURE USP_Usuarios_ValidarAcceso
    @Correo NVARCHAR(300)
AS
BEGIN
    SELECT IdUsuario, Nombre, Correo, Clave, IdRol, Estado
    FROM Usuarios
    WHERE Correo = @Correo AND Estado = 'A'  -- ✅ Cambio aplicado
END
GO

-- Ejemplo para USP_Cursos_ListarActivos:
ALTER PROCEDURE USP_Cursos_ListarActivos
AS
BEGIN
    SELECT IdCurso, Nombre, Sumilla, FechaInicio, FechaFin, IdProfesor, Costo, Estado
    FROM Cursos
    WHERE Estado = 'A'  -- ✅ Cambio aplicado
    ORDER BY FechaInicio DESC
END
GO

-- Ejemplo para USP_Matriculas_Registrar:
ALTER PROCEDURE USP_Matriculas_Registrar
    @IdAlumno INT,
    @IdCurso INT,
    @UsuarioRegistro VARCHAR(40) = 'Sistema'  -- ✅ Nuevo parámetro
AS
BEGIN
    INSERT INTO Matriculas (IdAlumno, IdCurso, Aprobado, EstadoMatricula, Modalidad, UsuarioRegistro, UsuarioModifica)
    VALUES (@IdAlumno, @IdCurso, 0, 1, 'P', @UsuarioRegistro, @UsuarioRegistro)  -- ✅ Nuevos campos
END
GO
```

---

## ⚠️ ADVERTENCIAS IMPORTANTES

1. **NO romper el sistema de certificados**: Los procedimientos relacionados con emisión de certificados y SkiaSharp NO deben modificar su lógica de negocio, solo agregar campos de auditoría.

2. **Compatibilidad hacia atrás**: Las propiedades `EstadoActivo` y `FechaMatricula` en las entidades C# mantienen compatibilidad con el código legacy.

3. **Testing obligatorio**: Después de actualizar cada SP, probar en entorno de desarrollo.

4. **Orden de ejecución**: Actualizar primero las entidades C#, luego los SPs, y finalmente probar los repositorios.

---

## 📞 CONTACTO
Si tienes dudas sobre algún procedimiento específico, consulta con el equipo de arquitectura antes de realizar cambios.
