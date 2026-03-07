using Inkillay.Certificados.Web.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inkillay.Certificados.Web.Controllers;

[Authorize(Roles = "Admin")]
public class CursosController : Controller
{
    private readonly ICursoRepository _cursoRepository;

    public CursosController(ICursoRepository cursoRepository)
    {
        _cursoRepository = cursoRepository;
    }

    public async Task<IActionResult> Index()
    {
        var cursos = await _cursoRepository.ListarTodosAsync();
        return View(cursos);
    }
}
