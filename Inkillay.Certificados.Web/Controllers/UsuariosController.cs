using Inkillay.Certificados.Web.Data.Repositories;
using Inkillay.Certificados.Web.Models.Entities;
using Inkillay.Certificados.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inkillay.Certificados.Web.Controllers;

[Authorize(Roles = "Admin")]
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
            return View(usuarios);
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
            var nuevoUsuario = new Usuarios
            {
                Nombre = model.Nombre.Trim(),
                Correo = model.Correo.Trim().ToLower(),
                Clave = model.Clave,
                IdRol = model.IdRol,
                Estado = true,
                FechaRegistro = DateTime.Now
            };

            var registrado = await _seguridadRepository.RegistrarUsuarioAsync(nuevoUsuario);
            if (registrado)
            {
                _logger.LogInformation("Usuario creado: {correo} por {usuario}", model.Correo, User.Identity?.Name);
                return Json(new { success = true, mensaje = "Usuario creado exitosamente" });
            }

            return Json(new { success = false, mensaje = "Error al crear el usuario" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar usuario: {correo}", model.Correo);
            return Json(new { success = false, mensaje = "Error interno del servidor" });
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
            Estado = usuario.Estado
        };

        return View(vm);
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
