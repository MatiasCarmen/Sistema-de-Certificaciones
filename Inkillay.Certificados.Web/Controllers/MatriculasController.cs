using SIGEC.Certificados.Web.Data.Repositories;
using SIGEC.Certificados.Web.Models.Entities;
using SIGEC.Certificados.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SIGEC.Certificados.Web.Controllers;

[Authorize(Roles = "Docente,Admin")]
public class MatriculasController : Controller
{
    private readonly ICursoRepository _cursoRepository;
    private readonly IMatriculaRepository _matriculaRepository;
    private readonly ISeguridadRepository _seguridadRepository;

    public MatriculasController(
        ICursoRepository cursoRepository,
        IMatriculaRepository matriculaRepository,
        ISeguridadRepository seguridadRepository)
    {
        _cursoRepository = cursoRepository;
        _matriculaRepository = matriculaRepository;
        _seguridadRepository = seguridadRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Registrar()
    {
        var cursos = await _cursoRepository.ListarCursosActivosAsync();
        var alumnos = await _seguridadRepository.ListarUsuariosAsync();
        
        var vm = new MatricularViewModel
        {
            Cursos = cursos,
            Alumnos = alumnos.Where(u => u.IdRol == 3)
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(int idAlumno, int idModulo)
    {
        if (idAlumno <= 0 || idModulo <= 0)
            return Json(new { success = false, mensaje = "Seleccione alumno y módulo válidos." });

        try
        {
            // Usando el nuevo repositorio y SPs de la Fase 3
            var nombreUsuario = User.Identity?.Name ?? "Sistema";
            var resultado = await _matriculaRepository.RegistrarMatriculaAsync(idModulo, idAlumno, nombreUsuario);
            
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
            return RedirectToAction("Index", "Docentes");

        try 
        {
            var cursoInfo = await _cursoRepository.ObtenerPorIdAsync(idCurso);
            if (cursoInfo == null) return NotFound();

            ViewData["NombreCurso"] = cursoInfo.Nombre;
            var matriculas = await _matriculaRepository.ListarAlumnosPorModuloAsync(idCurso);
            
            return View(matriculas ?? Enumerable.Empty<Matricula>());
        }
        catch (Exception)
        {
            return RedirectToAction("Index", "Docentes");
        }
    }
}
