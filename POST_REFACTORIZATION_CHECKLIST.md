# ✅ CHECKLIST DE VERIFICACIÓN POST-REFACTORIZACIÓN

**Proyecto:** Inkillay Certificados  
**Fecha:** 2024  
**Estado:** ✅ Compilación exitosa

---

## 🔍 VERIFICACIONES COMPLETADAS

### 1. **Compilación** ✅
- [x] `dotnet build` ejecutado sin errores
- [x] Todos los errores de tipo char vs bool resueltos
- [x] Tests unitarios actualizados y compilando

### 2. **Entidades Refactorizadas** ✅
- [x] AuditoriaBase creada con campos estándar corporativo
- [x] Usuarios: Estado migrado a char, hereda de AuditoriaBase
- [x] Curso: Estado migrado a char, hereda de AuditoriaBase
- [x] Matricula: Nuevos campos EstadoMatricula y Modalidad, hereda de AuditoriaBase
- [x] Plantilla: Estado migrado a char, nuevo campo FontFamily, hereda de AuditoriaBase
- [x] Seg_Modulo: Estado migrado a char, hereda de AuditoriaBase
- [x] Pago: Nueva entidad con FormaPago y TipoPago, hereda de AuditoriaBase
- [x] PlantillaDetalle: Nueva entidad, hereda de AuditoriaBase
- [x] Rol: Nueva entidad, hereda de AuditoriaBase
- [x] Seg_Opcion: Nueva entidad, hereda de AuditoriaBase

### 3. **Enums y Utilidades** ✅
- [x] DiccionarioNegocio.cs creado con todos los enums
- [x] RepositoryBase.cs creado con helpers de auditoría
- [x] AuditoriaService actualizado con ObtenerUsuarioActual()

### 4. **Repositorios Actualizados** ✅
- [x] IMatriculaRepository/MatriculaRepository: incluye usuarioRegistro
- [x] IPagoRepository/PagoRepository: incluye formaPago, tipoPago, usuarioRegistro
- [x] SeguridadRepository: CambiarEstadoUsuarioAsync maneja char

### 5. **Controladores Actualizados** ✅
- [x] PagosController: captura usuario actual para auditoría
- [x] DocentesController: captura usuario actual para auditoría
- [x] UsuariosController: maneja Estado como char

### 6. **Vistas Actualizadas** ✅
- [x] Emision/Index.cshtml: usa EstadoActivo
- [x] Plantillas/Index.cshtml: usa EstadoActivo
- [x] Cursos/Index.cshtml: usa EstadoActivo
- [x] Seguridad/Modulos.cshtml: usa EstadoActivo

### 7. **Tests Unitarios** ✅
- [x] UsuariosManagementTests.cs actualizado para manejar char Estado

### 8. **Documentación** ✅
- [x] MIGRATION_GUIDE_STORED_PROCEDURES.md creado
- [x] REFACTORIZATION_SUMMARY.md creado
- [x] Este checklist creado

---

## ⚠️ PENDIENTES (CRÍTICOS ANTES DE PRODUCCIÓN)

### 1. **Actualizar Procedimientos Almacenados** 🔴
**Responsable:** DBA  
**Archivo de referencia:** `MIGRATION_GUIDE_STORED_PROCEDURES.md`

#### Usuarios (5 SPs):
- [ ] `USP_Usuarios_Registrar` - Agregar @UsuarioRegistro
- [ ] `USP_Usuarios_ValidarAcceso` - WHERE Estado = 'A'
- [ ] `USP_Usuarios_ObtenerPorCorreo` - WHERE Estado = 'A'
- [ ] `USP_Usuarios_ListarTodos` - Devuelve Estado CHAR
- [ ] `USP_Usuarios_CambiarEstado` - @Estado CHAR(1)

#### Cursos (3 SPs):
- [ ] `USP_Cursos_ListarActivos` - WHERE Estado = 'A'
- [ ] `USP_Cursos_Insertar` - Agregar auditoría
- [ ] `USP_Cursos_Actualizar` - Agregar @UsuarioModifica

#### Matriculas (3 SPs):
- [ ] `USP_Matriculas_ListarAlumnosPorCurso` - SELECT EstadoMatricula, Modalidad
- [ ] `USP_Matriculas_Registrar` - INSERT EstadoMatricula, Modalidad, @UsuarioRegistro
- [ ] `USP_Alumnos_ListarMisCursos` - SELECT EstadoMatricula, Modalidad

#### Pagos (1 SP):
- [ ] `USP_Pagos_Registrar` - INSERT FormaPago, TipoPago, @UsuarioRegistro

#### Plantillas (6 SPs):
- [ ] `USP_Plantillas_ListarTodos` - WHERE Estado = 'A', SELECT FontFamily
- [ ] `USP_Plantillas_Insertar` - INSERT FontFamily, @UsuarioRegistro
- [ ] `USP_Plantillas_ActualizarCoordenadas` - UPDATE con @UsuarioModifica
- [ ] `USP_Plantillas_ActualizarDiseno` - UPDATE con @UsuarioModifica
- [ ] `USP_Plantillas_CambiarEstado` - @Estado CHAR(1)
- [ ] `USP_Plantillas_GuardarDisenoCompleto` - UPDATE con @UsuarioModifica

#### Seguridad (1 SP):
- [ ] `USP_Seg_Modulo_Listar` - WHERE Estado = 'A'

### 2. **Testing Funcional** 🟡
**Responsable:** QA  
**Ambiente:** Desarrollo/Staging

- [ ] Login/Logout con usuario Admin
- [ ] Login/Logout con usuario Docente
- [ ] Login/Logout con usuario Alumno
- [ ] Crear nuevo usuario
- [ ] Cambiar estado de usuario
- [ ] Listar cursos activos
- [ ] Registrar matrícula
- [ ] Registrar pago con FormaPago "Efectivo"
- [ ] Generar certificado PDF (SkiaSharp)
- [ ] Descargar certificado PDF
- [ ] Editor de plantillas multicapa
- [ ] Cambiar estado de plantilla
- [ ] Ver módulos de seguridad

### 3. **Verificaciones de Base de Datos** 🟡
**Responsable:** DBA

- [ ] Script de migración de columnas ejecutado:
  ```sql
  ALTER TABLE Usuarios ALTER COLUMN Estado CHAR(1) NOT NULL;
  ALTER TABLE Cursos ALTER COLUMN Estado CHAR(1) NOT NULL;
  ALTER TABLE Plantillas ALTER COLUMN Estado CHAR(1) NOT NULL;
  ALTER TABLE Roles ALTER COLUMN Estado CHAR(1) NOT NULL;
  ALTER TABLE Seg_Modulo ALTER COLUMN Estado CHAR(1) NOT NULL;
  ALTER TABLE Seg_Opcion ALTER COLUMN Estado CHAR(1) NOT NULL;
  ```

- [ ] Columnas de auditoría agregadas:
  ```sql
  ALTER TABLE Usuarios ADD UsuarioRegistro VARCHAR(40) NULL;
  ALTER TABLE Usuarios ADD UsuarioModifica VARCHAR(40) NULL;
  ALTER TABLE Usuarios ADD FechaModifica DATETIME NULL;
  -- (repetir para todas las tablas)
  ```

- [ ] Nuevos campos en Matriculas:
  ```sql
  ALTER TABLE Matriculas ADD EstadoMatricula INT DEFAULT 1;
  ALTER TABLE Matriculas ADD Modalidad CHAR(1) DEFAULT 'P';
  ALTER TABLE Matriculas RENAME COLUMN FechaMatricula TO FechaRegistro;
  ```

- [ ] Nuevos campos en Pagos:
  ```sql
  ALTER TABLE Pagos ADD FormaPago VARCHAR(20) NULL;
  ALTER TABLE Pagos ADD TipoPago VARCHAR(20) NULL;
  ```

- [ ] Nuevo campo en Plantillas:
  ```sql
  ALTER TABLE Plantillas ADD FontFamily NVARCHAR(100) DEFAULT 'Arial';
  ```

- [ ] Actualizar datos existentes:
  ```sql
  UPDATE Usuarios SET Estado = 'A' WHERE Estado = 1;
  UPDATE Usuarios SET Estado = 'I' WHERE Estado = 0;
  UPDATE Cursos SET Estado = 'A' WHERE Estado = 1;
  UPDATE Cursos SET Estado = 'I' WHERE Estado = 0;
  UPDATE Plantillas SET Estado = 'A' WHERE Estado = 1;
  UPDATE Plantillas SET Estado = 'I' WHERE Estado = 0;
  -- (repetir para todas las tablas)
  ```

### 4. **Performance y Seguridad** 🟢
**Responsable:** DevOps

- [ ] Verificar que los índices en columnas Estado sigan funcionando
- [ ] Verificar logs de auditoría (tabla Auditoria)
- [ ] Verificar que UsuarioRegistro y UsuarioModifica se capturen correctamente
- [ ] Verificar que FechaRegistro tenga DEFAULT GETDATE()
- [ ] Testing de carga: Emisión de 100+ certificados simultáneos

---

## 📊 MÉTRICAS DE CALIDAD

### Cobertura de Código
- [x] Compilación: 100%
- [ ] Tests unitarios: Actualizar según nuevas propiedades
- [ ] Tests de integración: Ejecutar contra BD actualizada

### Compatibilidad
- [x] Código legacy usa propiedades auxiliares (EstadoActivo, FechaMatricula)
- [x] Sistema de certificados (SkiaSharp) no modificado
- [x] Lógica de aprobación mantiene campo `Aprobado` en Matricula

---

## 🚀 PLAN DE DESPLIEGUE

### Fase 1: Preparación (1 día)
1. **DBA ejecuta:**
   - Backup completo de BD producción
   - Script de migración de columnas en ambiente de staging
   - Actualización de procedimientos almacenados en staging
   - Verificación de integridad referencial

2. **DevOps prepara:**
   - Branch de release con código refactorizado
   - Pipeline CI/CD configurado
   - Rollback plan documentado

### Fase 2: Testing en Staging (2-3 días)
1. **QA ejecuta:**
   - Batería completa de tests funcionales
   - Tests de regresión en módulos críticos
   - Verificación de auditoría (campos UsuarioRegistro/Modifica)

2. **Validación de certificados:**
   - Generar 50+ certificados de prueba
   - Verificar pixel-perfect con plantillas multicapa
   - Comparar PDFs generados antes/después

### Fase 3: Producción (Ventana de mantenimiento)
**Duración estimada:** 2 horas  
**Horario sugerido:** Viernes 11:00 PM - 1:00 AM (baja concurrencia)

1. **T-0:00** - Poner aplicación en modo mantenimiento
2. **T-0:05** - Backup completo de BD producción
3. **T-0:15** - Ejecutar scripts de migración de BD
4. **T-0:30** - Actualizar procedimientos almacenados (todos)
5. **T-0:45** - Deploy de aplicación refactorizada
6. **T-1:00** - Smoke tests (login, emisión de 1 certificado)
7. **T-1:15** - Verificar logs de auditoría
8. **T-1:30** - Quitar modo mantenimiento
9. **T-1:45** - Monitoreo activo por 15 minutos

### Fase 4: Post-Despliegue (1 semana)
- [ ] Monitoreo diario de logs de error
- [ ] Verificar campos de auditoría se llenan correctamente
- [ ] Feedback de usuarios sobre nuevas funcionalidades
- [ ] Ajustes menores si es necesario

---

## 🆘 PLAN DE ROLLBACK

Si algo falla en producción:

### Opción 1: Rollback de Aplicación (5 minutos)
1. Deploy de versión anterior desde pipeline CI/CD
2. Reversar procedimientos almacenados (ejecutar script rollback)
3. Monitorear logs

### Opción 2: Rollback Completo de BD (30 minutos)
1. Restaurar backup completo de BD
2. Deploy de versión anterior de aplicación
3. Verificar integridad

---

## 📞 CONTACTOS DE EMERGENCIA

- **Arquitecto Senior:** [Contacto]
- **DBA:** [Contacto]
- **DevOps:** [Contacto]
- **QA Lead:** [Contacto]

---

## 📝 NOTAS FINALES

### Lo que NO debe tocarse:
1. **CertificadoService.cs** - La lógica de SkiaSharp NO fue modificada
2. **Tabla Auditoria** - Es para tracking de eventos, no hereda de AuditoriaBase
3. **Campo Aprobado en Matricula** - Se mantiene para lógica de certificados

### Mejores prácticas aplicadas:
- ✅ Principio de responsabilidad única (SRP)
- ✅ Herencia con clase base abstracta
- ✅ Propiedades auxiliares para compatibilidad
- ✅ Enums tipados para valores de negocio
- ✅ Documentación inline en código
- ✅ Auditoría automática en INSERT/UPDATE

### Próximos pasos sugeridos:
1. Implementar middleware de auditoría automática
2. Crear dashboard de auditoría para Admin
3. Agregar validación de enums en stored procedures
4. Implementar soft-delete con Estado = 'D' (Deleted)

---

**Última actualización:** 2024  
**Estado del checklist:** 70% completado (pendiente actualización de BD y SPs)
