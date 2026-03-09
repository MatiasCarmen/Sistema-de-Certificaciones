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
    private readonly AuditoriaService _auditoriaService;
    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EmisionController(
        ICursoRepository cursoRepository,
        IMatriculaRepository matriculaRepository,
        ISeguridadRepository seguridadRepository,
        ICertificadoService certificadoService,
        AuditoriaService auditoriaService,
        IWebHostEnvironment hostEnvironment,
        IHttpContextAccessor httpContextAccessor)
    {
        _cursoRepository = cursoRepository;
        _matriculaRepository = matriculaRepository;
        _seguridadRepository = seguridadRepository;
        _certificadoService = certificadoService;
        _auditoriaService = auditoriaService;
        _hostEnvironment = hostEnvironment;
        _httpContextAccessor = httpContextAccessor;
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
        var alumnos = (await _matriculaRepository.ListarAlumnosPorCursoAsync(idCurso)).ToList();
        var curso = await _cursoRepository.ObtenerPorIdAsync(idCurso);

        if (curso == null)
            return Json(new { success = false, message = "Curso no encontrado." });

        if (!alumnos.Any())
            return Json(new { success = false, message = "No hay alumnos para emitir." });

        var plantillas = await _seguridadRepository.ListarPlantillasAsync();
        var plantilla = plantillas.FirstOrDefault(x => x.IdPlantilla == idPlantilla);
        if (plantilla == null)
            return Json(new { success = false, message = "Plantilla no encontrada." });

        var carpetaSalida = Path.Combine(_hostEnvironment.WebRootPath, "generated", idCurso.ToString());
        if (!Directory.Exists(carpetaSalida))
        {
            Directory.CreateDirectory(carpetaSalida);
        }

        int generados = 0;
        int omitidos = 0;

        foreach (var alumno in alumnos)
        {
            if (!(alumno.Aprobado && alumno.TotalPagado >= curso.Costo))
            {
                omitidos++;
                continue;
            }

            var bytes = _certificadoService.GenerarImagenCertificado(
                plantilla.RutaImagen,
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

    [HttpGet]
    public async Task<IActionResult> DescargarPdf(int idMatricula)
    {
        // 1. Obtener datos de la matrícula
        var matriculas = await _matriculaRepository.ListarAlumnosPorCursoAsync(0);
        var matricula = matriculas.FirstOrDefault(m => m.IdMatricula == idMatricula);

        if (matricula == null)
            return NotFound();

        // 2. Validar reglas de negocio (Pagado y Aprobado)
        if (!matricula.Aprobado || matricula.TotalPagado < matricula.CostoCurso)
            return Forbid("El alumno no cumple con los requisitos para descargar el certificado");

        try
        {
            // 3. Obtener la plantilla activa (la primera disponible)
            var plantillas = await _seguridadRepository.ListarPlantillasAsync();
            var plantilla = plantillas.FirstOrDefault();
            if (plantilla == null)
                return NotFound("No hay plantillas disponibles");

            // 4. Generar la imagen del certificado con SkiaSharp
            byte[] imagenCertificado = _certificadoService.GenerarImagenCertificado(
                plantilla.RutaImagen,
                matricula.NombreAlumno,
                plantilla.EjeX,
                plantilla.EjeY,
                plantilla.FontSize,
                plantilla.FontColor
            );

            // 5. Convertir imagen a PDF
            byte[] pdfBytes = _certificadoService.GenerarPdfDesdeImagen(imagenCertificado);

            // 6. Retornar archivo con nombre profesional
            string nombreArchivo = $"Certificado_{SanitizeFileName(matricula.NombreAlumno)}.pdf";

            // Registrar auditoría
            var idUsuario = User.FindFirst("IdUsuario");
            var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "DESCONOCIDA";

            await _auditoriaService.RegistrarAsync(
                idUsuario != null ? int.Parse(idUsuario.Value) : null,
                "DESCARGA PDF",
                "Certificados",
                $"Alumno: {matricula.NombreAlumno} - Curso: {matricula.NombreCurso}",
                ip
            );

            return File(pdfBytes, "application/pdf", nombreArchivo);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al generar el PDF: {ex.Message}");
        }
    }
}
