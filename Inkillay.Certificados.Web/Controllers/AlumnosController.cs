using Inkillay.Certificados.Web.Data.Repositories;
using Inkillay.Certificados.Web.Models.ViewModels;
using Inkillay.Certificados.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inkillay.Certificados.Web.Controllers;

[Authorize]
public class AlumnosController : Controller
{
    private readonly IMatriculaRepository _matriculaRepository;
    private readonly ISeguridadRepository _seguridadRepository;
    private readonly ILogger<AlumnosController> _logger;

    public AlumnosController(
        IMatriculaRepository matriculaRepository, 
        ISeguridadRepository seguridadRepository,
        ILogger<AlumnosController> logger)
    {
        _matriculaRepository = matriculaRepository;
        _seguridadRepository = seguridadRepository;
        _logger = logger;
    }

    #region Portal del Alumno (Rol=Alumno)

    [Authorize(Roles = "Alumno")]
    public async Task<IActionResult> MisCursos()
    {
        if (!int.TryParse(User.FindFirst("IdUsuario")?.Value, out var idAlumno))
        {
            return Unauthorized();
        }

        var misMatriculas = await _matriculaRepository.ListarModulosPorAlumnoAsync(idAlumno);
        return View(misMatriculas);
    }

    [Authorize(Roles = "Alumno")]
    public async Task<IActionResult> MisCertificados()
    {
        if (!int.TryParse(User.FindFirst("IdUsuario")?.Value, out var idAlumno))
        {
            return Unauthorized();
        }

        var misMatriculas = await _matriculaRepository.ListarModulosPorAlumnoAsync(idAlumno);
        
        // Filtrar matriculas listas para certificar (Aprobadas y Pagadas)
        var certificadosDisponibles = misMatriculas
            .Where(m => m.Aprobado && m.TotalPagado >= m.CostoCurso)
            .ToList();

        return View(certificadosDisponibles);
    }

    #endregion

    #region Gestión Administrativa (Rol=Admin, Docente)

    [Authorize(Roles = "Admin,Docente")]
    [HttpGet]
    public async Task<IActionResult> Padron()
    {
        try
        {
            var usuarios = await _seguridadRepository.ListarUsuariosAsync();
            var alumnos = usuarios
                .Where(u => u.IdRol == 3)
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
            _logger.LogError(ex, "Error al listar padrón de alumnos");
            return StatusCode(500, "Error al cargar el padrón");
        }
    }

    [Authorize(Roles = "Admin,Docente")]
    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var alumno = await _seguridadRepository.ObtenerAlumnoPorIdAsync(id);
        if (alumno == null)
            return NotFound("Alumno no encontrado");

        return View(alumno);
    }

    [Authorize(Roles = "Admin,Docente")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(EditarAlumnoViewModel model)
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
                return Json(new { success = true, mensaje = "Alumno actualizado exitosamente", redirectUrl = Url.Action("Padron") });
            }

            return Json(new { success = false, mensaje = "No se pudo actualizar." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar alumno ID: {id}", model.IdUsuario);
            return Json(new { success = false, mensaje = "ERROR: " + ex.Message });
        }
    }

    #endregion
}
