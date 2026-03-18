using SIGEC.Certificados.Web.Data.Repositories;
using SIGEC.Certificados.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SIGEC.Certificados.Web.Controllers;

[Authorize(Roles = "Docente,Admin")]
public class DocentesController : Controller
{
    private readonly ICursoRepository _cursoRepository;
    public DocentesController(ICursoRepository cursoRepository)
    {
        _cursoRepository = cursoRepository;
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
}
