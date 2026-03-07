using Inkillay.Certificados.Web.Data.Repositories;
using Inkillay.Certificados.Web.Models.Entities;
using Inkillay.Certificados.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inkillay.Certificados.Web.Controllers;

public class PlantillasController : Controller
{
    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly ISeguridadRepository _seguridadRepository;
    private readonly ICertificadoService _certificadoService;

    public PlantillasController(
        IWebHostEnvironment hostEnvironment,
        ISeguridadRepository seguridadRepository,
        ICertificadoService certificadoService)
    {
        _hostEnvironment = hostEnvironment;
        _seguridadRepository = seguridadRepository;
        _certificadoService = certificadoService;
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

        string rutaImagen = Path.Combine(_hostEnvironment.WebRootPath, "uploads", plantilla.RutaImagen);

        var imagenBytes = _certificadoService.GenerarImagenCertificado(
            rutaImagen,
            "MATIAS ADMINISTRADOR",
            plantilla.EjeX,
            plantilla.EjeY,
            plantilla.FontSize,
            plantilla.FontColor
        );

        return File(imagenBytes, "image/jpeg");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(string nombre, IFormFile imagen)
    {
        if (imagen == null || string.IsNullOrWhiteSpace(nombre))
            return Json(new { success = false, mensaje = "Datos incompletos" });

        try
        {
            string carpeta = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            string nombreArchivo = Guid.NewGuid() + Path.GetExtension(imagen.FileName);
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

            await _seguridadRepository.InsertarPlantillaAsync(plantilla);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, mensaje = ex.Message });
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
        try
        {
            var filas = await _seguridadRepository.ActualizarDisenoPlantillaAsync(id, x, y, fontSize, fontColor);
            return Json(new { success = filas > 0 });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, mensaje = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        var filas = await _seguridadRepository.CambiarEstadoPlantillaAsync(id);
        return Json(new { success = filas > 0 });
    }
}
