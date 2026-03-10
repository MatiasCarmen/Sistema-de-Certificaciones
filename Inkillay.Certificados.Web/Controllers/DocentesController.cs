using Inkillay.Certificados.Web.Data.Repositories;
using Inkillay.Certificados.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inkillay.Certificados.Web.Controllers;

[Authorize(Roles = "Docente,Admin")]
public class DocentesController : Controller
{
    private readonly ICursoRepository _cursoRepository;
    private readonly IMatriculaRepository _matriculaRepository;
    private readonly ISeguridadRepository _seguridadRepository;

    public DocentesController(
        ICursoRepository cursoRepository,
        IMatriculaRepository matriculaRepository,
        ISeguridadRepository seguridadRepository)
    {
        _cursoRepository = cursoRepository;
        _matriculaRepository = matriculaRepository;
        _seguridadRepository = seguridadRepository;
    }

    public async Task<IActionResult> Index()
    {
        if (!int.TryParse(User.FindFirst("IdUsuario")?.Value, out var idProfesor))
        {
            return Unauthorized();
        }

        var reporte = await _cursoRepository.ObtenerReporteDocenteAsync(idProfesor);
        return View(reporte);
    }

    [HttpGet]
    public async Task<IActionResult> Matricular()
    {
        var cursos = await _cursoRepository.ListarCursosActivosAsync();
        var usuarios = await _seguridadRepository.ListarUsuariosAsync();
        var alumnos = usuarios
            .Where(u => u.IdRol == 3)
            .Select(u => new UsuarioViewModel
            {
                IdUsuario = u.IdUsuario,
                Nombre = u.Nombre,
                Correo = u.Correo
            })
            .ToList();

        var vm = new MatricularViewModel
        {
            Cursos = cursos,
            Alumnos = alumnos
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Matricular(int idAlumno, int idCurso)
    {
        if (idAlumno <= 0)
            return Json(new { success = false, mensaje = "Seleccione un alumno." });

        if (idCurso <= 0)
            return Json(new { success = false, mensaje = "Seleccione un curso." });

        try
        {
            // Verificar que no esté ya matriculado
            var matriculasExistentes = await _matriculaRepository.ListarAlumnosPorCursoAsync(idCurso);
            if (matriculasExistentes.Any(m => m.IdAlumno == idAlumno))
                return Json(new { success = false, mensaje = "El alumno ya está matriculado en este curso." });

            var resultado = await _matriculaRepository.RegistrarMatriculaAsync(idAlumno, idCurso);
            if (resultado)
                return Json(new { success = true, mensaje = "Alumno matriculado exitosamente." });

            return Json(new { success = false, mensaje = "No se pudo registrar la matrícula." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, mensaje = $"Error: {ex.Message}" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> VerAlumnos(int idCurso)
    {
        if (idCurso <= 0)
            return Json(new { success = false, mensaje = "Curso no válido." });

        var alumnos = await _matriculaRepository.ListarAlumnosPorCursoAsync(idCurso);
        var curso = await _cursoRepository.ObtenerPorIdAsync(idCurso);

        ViewData["NombreCurso"] = curso?.Nombre ?? "Curso";
        ViewData["IdCurso"] = idCurso;

        return View(alumnos);
    }
}
