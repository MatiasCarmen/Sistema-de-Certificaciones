using SIGEC.Certificados.Web.Data.Repositories;
using SIGEC.Certificados.Web.Models.Entities;
using SIGEC.Certificados.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SIGEC.Certificados.Web.Controllers;

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

        var vm = new CrearModuloViewModel
        {
            Cursos = await _cursoRepository.ListarCursosActivosAsync(),
            Docentes = (await _seguridadRepository.ListarUsuariosAsync()).Where(u => u.IdRol == 2 && u.Estado == 'A'),
            IdCurso = modulo.IdCurso,
            IdDocente = modulo.IdDocente,
            EdicionCurso = modulo.EdicionCurso,
            CostoCurso = modulo.CostoCurso,
            CapacidadAlumno = modulo.CapacidadAlumno,
            FechaInicioMatricula = modulo.FechaInicioMatricula ?? DateTime.Now,
            FechaFinMatricula = modulo.FechaFinMatricula ?? DateTime.Now,
            FechaInicioClases = modulo.FechaInicioClases ?? DateTime.Now,
            Modalidad = modulo.Modalidad,
            EstadoMatricula = modulo.EstadoMatricula,
            Sumilla = modulo.Sumilla
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var modulo = await _moduloRepository.ObtenerPorIdAsync(id);
        if (modulo == null) return NotFound();

        var vm = new CrearModuloViewModel
        {
            Cursos = await _cursoRepository.ListarCursosActivosAsync(),
            Docentes = (await _seguridadRepository.ListarUsuariosAsync()).Where(u => u.IdRol == 2 && u.Estado == 'A'),
            IdCurso = modulo.IdCurso,
            IdDocente = modulo.IdDocente,
            EdicionCurso = modulo.EdicionCurso,
            CostoCurso = modulo.CostoCurso,
            CapacidadAlumno = modulo.CapacidadAlumno,
            FechaInicioMatricula = modulo.FechaInicioMatricula ?? DateTime.Now,
            FechaFinMatricula = modulo.FechaFinMatricula ?? DateTime.Now,
            FechaInicioClases = modulo.FechaInicioClases ?? DateTime.Now,
            Modalidad = modulo.Modalidad,
            EstadoMatricula = modulo.EstadoMatricula,
            Sumilla = modulo.Sumilla
        };

        ViewBag.IdModulo = modulo.IdModulo;
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int idModulo, Modulo modulo)
    {
        modulo.IdModulo = idModulo;
        if (ModelState.IsValid)
        {
            modulo.UsuarioModifica = User.Identity?.Name ?? "Sistema";
            var success = await _moduloRepository.ActualizarModuloAsync(modulo);

            if (success)
            {
                TempData["Success"] = "¡Módulo actualizado exitosamente!";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", "Error al actualizar la base de datos.");
        }

        var vm = new CrearModuloViewModel
        {
            Cursos = await _cursoRepository.ListarCursosActivosAsync(),
            Docentes = (await _seguridadRepository.ListarUsuariosAsync()).Where(u => u.IdRol == 2 && u.Estado == 'A'),
            IdCurso = modulo.IdCurso,
            IdDocente = modulo.IdDocente,
            EdicionCurso = modulo.EdicionCurso,
            CostoCurso = modulo.CostoCurso,
            CapacidadAlumno = modulo.CapacidadAlumno,
            FechaInicioMatricula = modulo.FechaInicioMatricula ?? DateTime.Now,
            FechaFinMatricula = modulo.FechaFinMatricula ?? DateTime.Now,
            FechaInicioClases = modulo.FechaInicioClases ?? DateTime.Now,
            Modalidad = modulo.Modalidad,
            EstadoMatricula = modulo.EstadoMatricula,
            Sumilla = modulo.Sumilla
        };

        ViewBag.IdModulo = idModulo;
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            var success = await _moduloRepository.EliminarModuloAsync(id);
            if (success) return Json(new { success = true });
            
            return Json(new { success = false, mensaje = "El registro no fue encontrado o no pudo eliminarse." });
        }
        catch (Exception)
        {
            return Json(new { success = false, mensaje = "No se puede eliminar porque cuenta con historiales, matrículas o certificaciones asociadas." });
        }
    }
}
