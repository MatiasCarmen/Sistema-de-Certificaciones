using Inkillay.Certificados.Web.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inkillay.Certificados.Web.Controllers;

[Authorize(Roles = "Alumno")]
public class AlumnosController : Controller
{
    private readonly IMatriculaRepository _matriculaRepository;

    public AlumnosController(IMatriculaRepository matriculaRepository)
    {
        _matriculaRepository = matriculaRepository;
    }

    public async Task<IActionResult> MisCursos()
    {
        if (!int.TryParse(User.FindFirst("IdUsuario")?.Value, out var idAlumno))
        {
            return Unauthorized();
        }

        var misMatriculas = await _matriculaRepository.ListarCursosPorAlumnoAsync(idAlumno);
        return View(misMatriculas);
    }

    public async Task<IActionResult> MisCertificados()
    {
        if (!int.TryParse(User.FindFirst("IdUsuario")?.Value, out var idAlumno))
        {
            return Unauthorized();
        }

        // Obtener los cursos del alumno
        var misMatriculas = await _matriculaRepository.ListarCursosPorAlumnoAsync(idAlumno);

        // Filtrar solo los que están aprobados y pagados (listos para certificar)
        var certificadosDisponibles = misMatriculas
            .Where(m => m.Aprobado && m.TotalPagado >= m.CostoCurso)
            .OrderByDescending(m => m.FechaMatricula)
            .ToList();

        return View(certificadosDisponibles);
    }
}
