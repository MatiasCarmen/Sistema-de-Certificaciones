using Inkillay.Certificados.Web.Data.Repositories;
using Inkillay.Certificados.Web.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Inkillay.Certificados.Web.Controllers;

public class PlantillasController : Controller
{
    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly ISeguridadRepository _seguridadRepository;

    public PlantillasController(IWebHostEnvironment hostEnvironment, ISeguridadRepository seguridadRepository)
    {
        _hostEnvironment = hostEnvironment;
        _seguridadRepository = seguridadRepository;
    }

    public async Task<IActionResult> Index()
    {
        var lista = await _seguridadRepository.ListarPlantillasAsync();
        return View(lista);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(string nombre, IFormFile imagen)
    {
        if (imagen == null || string.IsNullOrWhiteSpace(nombre))
        {
            return Json(new { success = false, mensaje = "Datos incompletos" });
        }

        try
        {
            string carpeta = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            // 1. Guardar archivo fisico
            string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName);
            string ruta = Path.Combine(carpeta, nombreArchivo);

            using (var stream = new FileStream(ruta, FileMode.Create))
            {
                await imagen.CopyToAsync(stream);
            }

            // 2. Guardar en BD
            var plantilla = new Plantilla
            {
                Nombre = nombre,
                RutaImagen = nombreArchivo
            };

            await _seguridadRepository.InsertarPlantillaAsync(plantilla);
            return Json(new { success = true, mensaje = "Plantilla subida con exito" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, mensaje = ex.Message });
        }
    }
}
