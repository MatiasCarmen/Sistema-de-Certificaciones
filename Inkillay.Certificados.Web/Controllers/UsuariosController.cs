using Inkillay.Certificados.Web.Data.Repositories;
using Inkillay.Certificados.Web.Models.Entities;
using Inkillay.Certificados.Web.Models.ViewModels;
using Inkillay.Certificados.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inkillay.Certificados.Web.Controllers;

[Authorize(Roles = "Admin,Docente")]
public class UsuariosController : Controller
{
    private readonly ISeguridadRepository _seguridadRepository;
    private readonly ILogger<UsuariosController> _logger;

    public UsuariosController(ISeguridadRepository seguridadRepository, ILogger<UsuariosController> logger)
    {
        _seguridadRepository = seguridadRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var usuarios = await _seguridadRepository.ListarUsuariosAsync();
            var vm = usuarios.Select(u => new UsuarioViewModel
            {
                IdUsuario = u.IdUsuario,
                Nombre = u.Nombre,
                Correo = u.Correo,
                IdRol = u.IdRol,
                NombreRol = u.IdRol switch
                {
                    1 => "Admin",
                    2 => "Docente",
                    3 => "Alumno",
                    _ => "Desconocido"
                },
                Estado = u.EstadoActivo,
                FechaRegistro = u.FechaRegistro ?? DateTime.MinValue
            }).ToList();
            return View(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar usuarios");
            return StatusCode(500, "Error al cargar usuarios");
        }
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CrearUsuarioViewModel model)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, mensaje = "Datos inválidos" });

        // Validar que el correo no esté duplicado
        var correoExiste = await _seguridadRepository.CorreoExisteAsync(model.Correo);
        if (correoExiste)
            return Json(new { success = false, mensaje = "El correo ya está registrado" });

        // Validar que la contraseña no esté vacía
        if (string.IsNullOrWhiteSpace(model.Clave) || model.Clave.Length < 6)
            return Json(new { success = false, mensaje = "La contraseña debe tener al menos 6 caracteres" });

        try
        {
            bool registrado = false;
            string usuarioAdmin = User.Identity?.Name ?? "Sistema";

            if (model.IdRol == 3)
            {
                // Hasheamos la clave in-situ antes de pasar al ViewModel hacia Dapper
                model.Clave = HashHelper.HashPassword(model.Clave);
                registrado = await _seguridadRepository.RegistrarAlumnoCompletoAsync(model, usuarioAdmin);
            }
            else
            {
                var nuevoUsuario = new Usuarios
                {
                    Nombre = model.Nombre.Trim(),
                    Correo = model.Correo.Trim().ToLower(),
                    Clave = HashHelper.HashPassword(model.Clave),
                    IdRol = model.IdRol,
                    Estado = 'A',  // ✅ Cambiado de true a 'A' según estándar corporativo
                    UsuarioRegistro = usuarioAdmin,
                    FechaRegistro = DateTime.Now
                };

                registrado = await _seguridadRepository.RegistrarUsuarioAsync(nuevoUsuario);
            }

            if (registrado)
            {
                _logger.LogInformation("Usuario creado: {correo} por {usuario}", model.Correo, User.Identity?.Name);
                var redirectUrl = model.IdRol == 3 
                    ? Url.Action("IndexAlumnos", "Usuarios") 
                    : Url.Action("Index");

                return Json(new { success = true, mensaje = "Usuario creado exitosamente", redirectUrl });
            }

            return Json(new { success = false, mensaje = "Error al crear el usuario en la base de datos." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar usuario: {correo}", model.Correo);
            return Json(new { success = false, mensaje = "ERROR REAL: " + ex.Message + " | INTERNO: " + ex.InnerException?.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> IndexAlumnos()
    {
        try
        {
            var usuarios = await _seguridadRepository.ListarUsuariosAsync();
            var alumnos = usuarios
                .Where(u => u.IdRol == 3) // Solo Rol 3 (Alumnos)
                .Select(u => new UsuarioViewModel
                {
                    IdUsuario = u.IdUsuario,
                    Nombre = u.Nombre,
                    Correo = u.Correo,
                    IdRol = u.IdRol,
                    NombreRol = "Alumno",
                    Estado = u.EstadoActivo,
                    FechaRegistro = u.FechaRegistro ?? DateTime.MinValue
                }).ToList();

            return View(alumnos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar alumnos");
            return StatusCode(500, "Error al cargar el padrón de alumnos");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var usuario = await _seguridadRepository.ObtenerUsuarioPorIdAsync(id);
        if (usuario == null)
            return NotFound();

        var vm = new EditarUsuarioViewModel
        {
            IdUsuario = usuario.IdUsuario,
            Nombre = usuario.Nombre,
            Correo = usuario.Correo,
            IdRol = usuario.IdRol,
            Estado = usuario.EstadoActivo 
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> EditAlumno(int id)
    {
        var alumno = await _seguridadRepository.ObtenerAlumnoPorIdAsync(id);
        if (alumno == null)
            return NotFound("Alumno no encontrado");

        return View(alumno);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAlumno(EditarAlumnoViewModel model)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, mensaje = "Datos inválidos" });

        try
        {
            var usuarioModifica = User.Identity?.Name ?? "Sistema";
            var actualizado = await _seguridadRepository.ActualizarAlumnoCompletoAsync(model, usuarioModifica);

            if (actualizado)
            {
                _logger.LogInformation("Alumno modificado: {id} por {usuario}", model.IdUsuario, usuarioModifica);
                return Json(new { success = true, mensaje = "Alumno actualizado exitosamente", redirectUrl = Url.Action("IndexAlumnos") });
            }

            return Json(new { success = false, mensaje = "No se pudo actualizar el alumno. Verifique los datos." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar alumno ID: {id}", model.IdUsuario);
            return Json(new { success = false, mensaje = "ERROR: " + ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int idUsuario, bool estado)
    {
        try
        {
            var filas = await _seguridadRepository.CambiarEstadoUsuarioAsync(idUsuario, estado);
            if (filas > 0)
            {
                _logger.LogInformation("Estado de usuario {id} cambió a {estado} por {usuario}", 
                    idUsuario, estado ? "Activo" : "Inactivo", User.Identity?.Name);
                return Json(new { success = true, mensaje = "Estado actualizado" });
            }

            return Json(new { success = false, mensaje = "Error al actualizar estado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar estado de usuario {id}", idUsuario);
            return Json(new { success = false, mensaje = "Error interno" });
        }
    }
}
