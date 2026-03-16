using Inkillay.Certificados.Web.Data.Repositories;
using Inkillay.Certificados.Web.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inkillay.Certificados.Web.Controllers;

[Authorize(Roles = "Admin,Docente")]
public class ModulosController : Controller
{
    private readonly IModuloRepository _moduloRepository;
    private readonly ICursoRepository _cursoRepository;

    public ModulosController(IModuloRepository moduloRepository, ICursoRepository cursoRepository)
    {
        _moduloRepository = moduloRepository;
        _cursoRepository = cursoRepository;
    }

    // 1. Ver todos los Módulos
    public async Task<IActionResult> Index()
    {
        var modulos = await _moduloRepository.ListarTodosAsync();
        return View(modulos);
    }

    // 2. Formulario de Creación de Módulo
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Cursos = await _cursoRepository.ListarCursosActivosAsync();
        return View(new Modulo { FechaInicioMatricula = DateTime.Now });
    }

    // 3. Crear Módulo (Grupo)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Modulo modulo)
    {
        if (ModelState.IsValid)
        {
            modulo.UsuarioRegistro = User.Identity?.Name ?? "Sistema";
            var idGenerado = await _moduloRepository.RegistrarModuloAsync(modulo);

            if (idGenerado > 0)
            {
                TempData["Success"] = "¡Módulo creado exitosamente!";
                return RedirectToAction(nameof(Index));
            }
        }

        ViewBag.Cursos = await _cursoRepository.ListarCursosActivosAsync();
        return View(modulo);
    }
}
