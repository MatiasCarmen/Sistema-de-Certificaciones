using Inkillay.Certificados.Web.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inkillay.Certificados.Web.Controllers;

[Authorize(Roles = "Alumno")]
public class AlumnosController : Controller
{
    private readonly IModuloRepository _ModuloRepository;

    public AlumnosController(IModuloRepository ModuloRepository)
    {
        _ModuloRepository = ModuloRepository;
    }

    public async Task<IActionResult> MisCursos()
    {
        if (!int.TryParse(User.FindFirst("IdUsuario")?.Value, out var idAlumno))
        {
            return Unauthorized();
        }

        var misModulos = await _ModuloRepository.ListarCursosPorAlumnoAsync(idAlumno);
        return View(misModulos);
    }

    public async Task<IActionResult> MisCertificados()
    {
        if (!int.TryParse(User.FindFirst("IdUsuario")?.Value, out var idAlumno))
        {
            return Unauthorized();
        }

        // Obtener los cursos del alumno
        var misModulos = await _ModuloRepository.ListarCursosPorAlumnoAsync(idAlumno);

        // Filtrar solo los que están aprobados y pagados (listos para certificar)
        var certificadosDisponibles = misModulos
            .Where(m => m.Aprobado && m.TotalPagado >= m.CostoCurso)
            .OrderByDescending(m => m.FechaModulo)
            .ToList();

        return View(certificadosDisponibles);
    }
}
