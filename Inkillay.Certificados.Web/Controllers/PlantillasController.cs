using Inkillay.Certificados.Web.Data.Repositories;
using Inkillay.Certificados.Web.Models.Entities;
using Inkillay.Certificados.Web.Models.ViewModels;
using Inkillay.Certificados.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Inkillay.Certificados.Web.Controllers;

[Authorize(Roles = "Admin")]
public class PlantillasController : Controller
{
    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly IPlantillaRepository _plantillaRepository;
    private readonly ICertificadoService _certificadoService;
    private readonly ILogger<PlantillasController> _logger;

    public PlantillasController(
        IWebHostEnvironment hostEnvironment,
        IPlantillaRepository plantillaRepository,
        ICertificadoService certificadoService,
        ILogger<PlantillasController> logger)
    {
        _hostEnvironment = hostEnvironment;
        _plantillaRepository = plantillaRepository;
        _certificadoService = certificadoService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var lista = await _plantillaRepository.ListarPlantillasAsync();
        return View(lista);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Editor(int id)
    {
        var plantillas = await _plantillaRepository.ListarPlantillasAsync();
        var plantilla = plantillas.FirstOrDefault(p => p.IdPlantilla == id);
        if (plantilla == null) return NotFound();

        var detalles = (await _plantillaRepository.ListarDetallesPlantillaAsync(id)).ToList();

        // Backward compatibility: if no detalles exist but plantilla has coordinates, create a virtual layer
        if (detalles.Count == 0 && (plantilla.EjeX > 0 || plantilla.EjeY > 0))
        {
            detalles.Add(new PlantillaDetalleDTO
            {
                Texto = "NOMBRE ALUMNO",
                X = plantilla.EjeX,
                Y = plantilla.EjeY,
                FontSize = plantilla.FontSize,
                FontColor = plantilla.FontColor,
                EsPrincipal = 1,
                Orden = 0
            });
        }

        var vm = new EditorPlantillaViewModel
        {
            Plantilla = plantilla,
            Detalles = detalles
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Previsualizar(int id)
    {
        var plantillas = await _plantillaRepository.ListarPlantillasAsync();
        var plantilla = plantillas.FirstOrDefault(p => p.IdPlantilla == id);
        if (plantilla == null) return NotFound();

        try
        {
            var detalles = (await _plantillaRepository.ListarDetallesPlantillaAsync(id)).ToList();
            byte[] imagenBytes;

            if (detalles.Count > 0)
            {
                imagenBytes = _certificadoService.GenerarImagenCertificadoMulticapa(
                    plantilla.RutaImagen,
                    "NOMBRE DE PRUEBA",
                    detalles
                );
            }
            else
            {
                imagenBytes = _certificadoService.GenerarImagenCertificado(
                    plantilla.RutaImagen,
                    "NOMBRE DE PRUEBA",
                    plantilla.EjeX,
                    plantilla.EjeY,
                    plantilla.FontSize,
                    plantilla.FontColor
                );
            }

            return File(imagenBytes, "image/jpeg");
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogWarning(ex, "Plantilla no encontrada en uploads. IdPlantilla={IdPlantilla}", plantilla.IdPlantilla);
            return NotFound("Plantilla no encontrada en uploads.");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Intento de acceso a ruta no permitida. IdPlantilla={IdPlantilla}", plantilla.IdPlantilla);
            return BadRequest("Ruta de plantilla invalida.");
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(string nombre, IFormFile imagen)
    {
        if (imagen == null || imagen.Length == 0)
            return Json(new { success = false, mensaje = "Debes subir una imagen" });

        var extension = Path.GetExtension(imagen.FileName).ToLowerInvariant();
        var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png" };
        if (!extensionesPermitidas.Contains(extension))
            return Json(new { success = false, mensaje = "Solo se permiten JPG o PNG" });

        if (imagen.Length > 5 * 1024 * 1024)
            return Json(new { success = false, mensaje = "La imagen es muy pesada (max 5MB)" });

        if (string.IsNullOrWhiteSpace(nombre))
            return Json(new { success = false, mensaje = "Datos incompletos" });

        try
        {
            string carpeta = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            string nombreArchivo = $"{Guid.NewGuid()}{extension}";
            string ruta = Path.Combine(carpeta, nombreArchivo);

            await using (var stream = new FileStream(ruta, FileMode.Create))
            {
                await imagen.CopyToAsync(stream);
            }

            var plantilla = new Plantilla
            {
                Nombre = nombre,
                RutaImagen = nombreArchivo
            };

            await _plantillaRepository.InsertarPlantillaAsync(plantilla);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al subir plantilla");
            return Json(new { success = false, mensaje = "Error interno del servidor" });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActualizarDiseno(int id, int x, int y, int fontSize, string fontColor)
    {
        var filas = await _plantillaRepository.ActualizarDisenoPlantillaAsync(id, x, y, fontSize, fontColor);
        return Json(new { success = filas > 0 });
    }

    [HttpPost]
    public async Task<IActionResult> GuardarDiseno([FromBody] GuardarDisenoRequest request)
    {
        if (request == null || request.id <= 0)
            return Json(new { success = false, mensaje = "Datos inválidos" });

        try
        {
            var filas = await _plantillaRepository.ActualizarDisenoPlantillaAsync(
                request.id,
                int.Parse(request.ejeX.ToString()),
                int.Parse(request.ejeY.ToString()),
                int.Parse(request.fontSize.ToString()),
                request.fontColor
            );

            if (filas > 0)
            {
                _logger.LogInformation("Diseño de plantilla actualizado. IdPlantilla={IdPlantilla}", request.id);
                return Json(new { success = true, mensaje = "Diseño actualizado correctamente" });
            }

            return Json(new { success = false, mensaje = "No se pudieron guardar los cambios" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error guardando diseño. IdPlantilla={IdPlantilla}", request.id);
            return Json(new { success = false, mensaje = "Error al guardar: " + ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GuardarTodo(int id, string json)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json))
                return Json(new { success = false, mensaje = "No se recibieron datos de capas" });

            // Validate it's actual JSON before sending to DB
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var detalles = JsonSerializer.Deserialize<List<PlantillaDetalleDTO>>(json, options);
            if (detalles == null || detalles.Count == 0)
                return Json(new { success = false, mensaje = "No hay capas para guardar" });

            var ok = await _plantillaRepository.GuardarDisenoCompletoAsync(id, detalles);
            return Json(new { success = ok, mensaje = ok ? "Diseño guardado correctamente" : "No se pudieron guardar los cambios" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error guardando diseno completo. IdPlantilla={IdPlantilla}", id);
            return Json(new { success = false, mensaje = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        var filas = await _plantillaRepository.CambiarEstadoPlantillaAsync(id);
        return Json(new { success = filas > 0 });
    }
}


