using Inkillay.Certificados.Web.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inkillay.Certificados.Web.Controllers;

[Authorize]
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
}
