using Inkillay.Certificados.Web.Data.Repositories;
using Inkillay.Certificados.Web.Models.Entities;
using Inkillay.Certificados.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inkillay.Certificados.Web.Controllers;

[Authorize(Roles = "Admin,Docente")]
public class ModulosController : Controller
{
    private readonly IModuloRepository _moduloRepository;
    private readonly ICursoRepository _cursoRepository;
    private readonly ISeguridadRepository _seguridadRepository; // Usamos Seguridad para los Docentes

    public ModulosController(
        IModuloRepository moduloRepository,
        ICursoRepository cursoRepository,
        ISeguridadRepository seguridadRepository)
    {
        _moduloRepository = moduloRepository;
        _cursoRepository = cursoRepository;
        _seguridadRepository = seguridadRepository;
    }

    // Listado de Módulos
    public async Task<IActionResult> Index()
    {
        var modulos = await _moduloRepository.ListarTodosAsync();
        return View(modulos);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var cursos = await _cursoRepository.ListarCursosActivosAsync();
        var usuarios = await _seguridadRepository.ListarUsuariosAsync();
        var docentes = usuarios.Where(u => u.IdRol == 2 && u.Estado == 'A').ToList();

        var vm = new CrearModuloViewModel
        {
            Cursos = cursos,
            Docentes = docentes,
            FechaInicioMatricula = DateTime.Now,
            EstadoMatricula = '1' // Estado inicial "Creado"
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Modulo modulo)
    {
        if (ModelState.IsValid)
        {
            // Auditoría: Quién está creando el registro
            modulo.UsuarioRegistro = User.Identity?.Name ?? "Sistema";

            // Llamamos al repositorio que ya configuramos con el SP: USP_Modulos_Insertar
            var idGenerado = await _moduloRepository.RegistrarModuloAsync(modulo);

            if (idGenerado > 0)
            {
                TempData["Success"] = "¡Módulo creado exitosamente!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Error al intentar guardar en la base de datos.");
        }

        // Si el modelo es inválido, recargamos los ViewBags para que no explote la vista
        ViewBag.Cursos = await _cursoRepository.ListarCursosActivosAsync();
        var usuariosRedo = await _seguridadRepository.ListarTodosAsync();
        ViewBag.Docentes = usuariosRedo.Where(u => u.IdRol == 2 && u.Estado == 'A');

        return View(modulo);
    }
}