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

    public async Task<IActionResult> Details(int id)
    {
        var curso = await _cursoRepository.ObtenerPorIdAsync(id);
        if (curso is null)
        {
            return NotFound();
        }
        return View(curso);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Imagen(int id)
    {
        var curso = await _cursoRepository.ObtenerPorIdAsync(id);
        if (string.IsNullOrWhiteSpace(curso?.Imagen))
        {
            return NotFound();
        }

        var raw = curso.Imagen.Trim()
            .Replace("\r", "")
            .Replace("\n", "");

        if (raw.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            var base64Marker = "base64,";
            var base64Index = raw.IndexOf(base64Marker, StringComparison.OrdinalIgnoreCase);
            if (base64Index < 0)
            {
                return BadRequest("Formato base64 inválido");
            }

            var header = raw.Substring(5, base64Index - 5);
            var mimeType = header.TrimEnd(';');
            var base64 = raw.Substring(base64Index + base64Marker.Length);

            
            base64 = base64.Replace(" ", "+");

            
            int mod4 = base64.Length % 4;
            if (mod4 > 0)
            {
                base64 += new string('=', 4 - mod4);
            }

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(base64);
            }
            catch (Exception ex)
            {
                Response.Headers["X-Base64-Error"] = ex.Message;
                return BadRequest("Error al decodificar imagen");
            }

            return File(bytes, string.IsNullOrWhiteSpace(mimeType) ? "application/octet-stream" : mimeType);
        }

        if (raw.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || raw.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            return Redirect(raw);
        }

        return NotFound();
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

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var curso = await _cursoRepository.ObtenerPorIdAsync(id);
        if (curso == null) return NotFound();
        return View(curso);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Curso curso)
    {
        if (ModelState.IsValid)
        {
            curso.UsuarioModifica = User.Identity?.Name ?? "Sistema";
            var resultado = await _cursoRepository.ActualizarCursoAsync(curso);

            if (resultado)
            {
                TempData["Success"] = "¡Curso actualizado exitosamente!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "No se pudo actualizar en la base de datos.");
        }
        return View(curso);
    }
}
