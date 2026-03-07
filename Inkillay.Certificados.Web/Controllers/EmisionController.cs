using Inkillay.Certificados.Web.Data.Repositories;
using Inkillay.Certificados.Web.Models.ViewModels;
using Inkillay.Certificados.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inkillay.Certificados.Web.Controllers;

[Authorize]
public class EmisionController : Controller
{
    private readonly ICursoRepository _cursoRepository;
    private readonly IMatriculaRepository _matriculaRepository;
    private readonly ISeguridadRepository _seguridadRepository;
    private readonly ICertificadoService _certificadoService;
    private readonly IWebHostEnvironment _hostEnvironment;

    public EmisionController(
        ICursoRepository cursoRepository,
        IMatriculaRepository matriculaRepository,
        ISeguridadRepository seguridadRepository,
        ICertificadoService certificadoService,
        IWebHostEnvironment hostEnvironment)
    {
        _cursoRepository = cursoRepository;
        _matriculaRepository = matriculaRepository;
        _seguridadRepository = seguridadRepository;
        _certificadoService = certificadoService;
        _hostEnvironment = hostEnvironment;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? idCurso)
    {
        var cursos = await _cursoRepository.ListarCursosActivosAsync();
        var alumnos = idCurso.HasValue
            ? await _matriculaRepository.ListarAlumnosPorCursoAsync(idCurso.Value)
            : Enumerable.Empty<Models.Entities.Matricula>();

        var vm = new EmisionIndexViewModel
        {
            IdCursoSeleccionado = idCurso,
            Cursos = cursos,
            Alumnos = alumnos
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerarCertificados(int idCurso, int idPlantilla)
    {
        // 1. Traemos la lista de alumnos con sus pagos
        var alumnos = (await _matriculaRepository.ListarAlumnosPorCursoAsync(idCurso)).ToList();
        var curso = await _cursoRepository.ObtenerPorIdAsync(idCurso);

        if (curso == null)
        {
            return Json(new { success = false, message = "Curso no encontrado." });
        }

        if (!alumnos.Any())
        {
            return Json(new { success = false, message = "No hay alumnos para emitir." });
        }

        var plantillas = await _seguridadRepository.ListarPlantillasAsync();
        var plantilla = plantillas.FirstOrDefault(x => x.IdPlantilla == idPlantilla);
        if (plantilla == null)
        {
            return Json(new { success = false, message = "Plantilla no encontrada." });
        }

        var rutaPlantilla = Path.Combine(_hostEnvironment.WebRootPath, "uploads", plantilla.RutaImagen);
        var carpetaSalida = Path.Combine(_hostEnvironment.WebRootPath, "generated", idCurso.ToString());
        if (!Directory.Exists(carpetaSalida))
        {
            Directory.CreateDirectory(carpetaSalida);
        }

        int generados = 0;
        int omitidos = 0;

        foreach (var alumno in alumnos)
        {
            // REGLA DE NEGOCIO: Solo emite si esta aprobado y pago el total
            if (!(alumno.Aprobado && alumno.TotalPagado >= curso.Costo))
            {
                omitidos++;
                continue;
            }

            var bytes = _certificadoService.GenerarImagenCertificado(
                rutaPlantilla,
                alumno.NombreAlumno,
                plantilla.EjeX,
                plantilla.EjeY,
                plantilla.FontSize,
                plantilla.FontColor
            );

            var nombreArchivo = $"{alumno.IdAlumno}_{SanitizeFileName(alumno.NombreAlumno)}.jpg";
            var rutaSalida = Path.Combine(carpetaSalida, nombreArchivo);
            await System.IO.File.WriteAllBytesAsync(rutaSalida, bytes);
            generados++;

            // TODO: Registrar en BD que el certificado ya esta disponible
        }

        return Json(new
        {
            success = true,
            message = $"Se procesaron {generados} certificados correctamente.",
            totalGenerados = generados,
            totalOmitidos = omitidos,
            outputPath = $"/generated/{idCurso}"
        });
    }

    private static string SanitizeFileName(string value)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(c, '_');
        }

        return value.Replace(' ', '_');
    }
}
