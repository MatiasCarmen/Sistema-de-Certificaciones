using Inkillay.Certificados.Web.Data.Repositories;
using Inkillay.Certificados.Web.Models.Entities;
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

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Curso curso)
    {
        if (ModelState.IsValid)
        {
            curso.UsuarioRegistro = User.Identity?.Name ?? "Sistema";

            int idGenerado = await _cursoRepository.RegistrarCursoAsync(curso);

            if (idGenerado > 0)
            {
                TempData["Success"] = "¡Curso creado exitosamente!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "No se pudo registrar en la base de datos.");
        }
        return View(curso);
    }
}
