using Inkillay.Certificados.Web.Data.Repositories;
using Inkillay.Certificados.Web.Models.Entities;
using Inkillay.Certificados.Web.Services;
using Inkillay.Certificados.Web.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inkillay.Certificados.Web.Controllers;

public class PlantillasController : Controller
{
    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly ISeguridadRepository _seguridadRepository;
    private readonly ICertificadoService _certificadoService;
    private readonly ILogger<PlantillasController> _logger;

    public PlantillasController(
        IWebHostEnvironment hostEnvironment,
        ISeguridadRepository seguridadRepository,
        ICertificadoService certificadoService,
        ILogger<PlantillasController> logger)
    {
        _hostEnvironment = hostEnvironment;
        _seguridadRepository = seguridadRepository;
        _certificadoService = certificadoService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var lista = await _seguridadRepository.ListarPlantillasAsync();
        return View(lista);
    }

    [HttpGet]
    public async Task<IActionResult> Editor(int id)
    {
        var plantillas = await _seguridadRepository.ListarPlantillasAsync();
        var plantilla = plantillas.FirstOrDefault(p => p.IdPlantilla == id);
        if (plantilla == null) return NotFound();
        return View(plantilla);
    }

    [HttpGet]
    public async Task<IActionResult> Previsualizar(int id)
    {
        var plantillas = await _seguridadRepository.ListarPlantillasAsync();
        var plantilla = plantillas.FirstOrDefault(p => p.IdPlantilla == id);

        if (plantilla == null) return NotFound();

        var imagenBytes = _certificadoService.GenerarImagenCertificado(
            plantilla.RutaImagen,
            "MATIAS ADMINISTRADOR",
            plantilla.EjeX,
            plantilla.EjeY,
            plantilla.FontSize,
            plantilla.FontColor
        );

        return File(imagenBytes, "image/jpeg");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(string nombre, IFormFile imagen)
    {
        // 1. Validación integral del archivo usando FileValidationHelper
        var (isValid, errorMessage) = FileValidationHelper.ValidateImageFile(imagen);
        if (!isValid)
        {
            _logger.LogWarning("Intento de subida de archivo inválido: {error}", errorMessage);
            return Json(new { success = false, mensaje = errorMessage });
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            _logger.LogWarning("Intento de crear plantilla sin nombre");
            return Json(new { success = false, mensaje = "Datos incompletos" });
        }

        try
        {
            string carpeta = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            string extension = Path.GetExtension(imagen.FileName).ToLowerInvariant();
            string nombreArchivo = $"{Guid.NewGuid()}{extension}";
            string ruta = Path.Combine(carpeta, nombreArchivo);

            // 2. Garantizar que la ruta no escape del directorio permitido (defensa en profundidad)
            var rutaCompleta = Path.GetFullPath(ruta);
            var carpetaCompleta = Path.GetFullPath(carpeta);
            if (!rutaCompleta.StartsWith(carpetaCompleta, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError("Intento de path traversal detectado al crear plantilla");
                return Json(new { success = false, mensaje = "Error de validación de ruta" });
            }

            await using (var stream = new FileStream(ruta, FileMode.Create))
            {
                await imagen.CopyToAsync(stream);
            }

            var plantilla = new Plantilla
            {
                Nombre = nombre,
                RutaImagen = nombreArchivo
            };

            await _seguridadRepository.InsertarPlantillaAsync(plantilla);
            _logger.LogInformation("Plantilla '{nombre}' creada exitosamente por {usuario}", nombre, User.Identity?.Name ?? "Sistema");
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al subir plantilla: {nombre}", nombre);
            return Json(new { success = false, mensaje = "Error interno del servidor" });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GuardarEjes(int id, int x, int y)
    {
        var filas = await _seguridadRepository.ActualizarCoordenadasAsync(id, x, y);
        return Json(new { success = filas > 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActualizarDiseno(int id, int x, int y, int fontSize, string fontColor)
    {
        var filas = await _seguridadRepository.ActualizarDisenoPlantillaAsync(id, x, y, fontSize, fontColor);
        return Json(new { success = filas > 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        var filas = await _seguridadRepository.CambiarEstadoPlantillaAsync(id);
        return Json(new { success = filas > 0 });
    }
}
