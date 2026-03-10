using Inkillay.Certificados.Web.Data.Repositories;
using Inkillay.Certificados.Web.Models.Entities;
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
        var plantillas = await _seguridadRepository.ListarPlantillasAsync();
        var alumnos = idCurso.HasValue
            ? await _matriculaRepository.ListarAlumnosPorCursoAsync(idCurso.Value)
            : Enumerable.Empty<Models.Entities.Matricula>();

        var vm = new EmisionIndexViewModel
        {
            IdCursoSeleccionado = idCurso,
            Cursos = cursos,
            Alumnos = alumnos,
            Plantillas = plantillas
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerarCertificados(int idCurso, int idPlantilla)
    {
        // Validar que se haya seleccionado un curso
        if (idCurso <= 0)
            return Json(new { success = false, message = "Por favor, seleccione un curso primero." });

        if (idPlantilla <= 0)
            return Json(new { success = false, message = "Por favor, seleccione una plantilla." });

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

        var detalles = (await _seguridadRepository.ListarDetallesPlantillaAsync(idPlantilla)).ToList();

        foreach (var alumno in alumnos)
        {
            if (!(alumno.Aprobado && alumno.TotalPagado >= curso.Costo))
            {
                omitidos++;
                continue;
            }

            byte[] bytes;
            if (detalles.Count > 0)
            {
                bytes = _certificadoService.GenerarImagenCertificadoMulticapa(
                    plantilla.RutaImagen,
                    alumno.NombreAlumno,
                    detalles
                );
            }
            else
            {
                bytes = _certificadoService.GenerarImagenCertificado(
                    plantilla.RutaImagen,
                    alumno.NombreAlumno,
                    plantilla.EjeX,
                    plantilla.EjeY,
                    plantilla.FontSize,
                    plantilla.FontColor
                );
            }

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
        if (idMatricula <= 0)
            return BadRequest("ID de matrícula inválido.");

        // 1. Buscar la matrícula en todos los cursos
        // Intentar obtener datos del alumno usando el id del usuario logueado
        Matricula? matricula = null;
        var cursos = await _cursoRepository.ListarCursosActivosAsync();
        foreach (var curso in cursos)
        {
            var alumnos = await _matriculaRepository.ListarAlumnosPorCursoAsync(curso.IdCurso);
            matricula = alumnos.FirstOrDefault(m => m.IdMatricula == idMatricula);
            if (matricula != null) break;
        }

        if (matricula == null)
            return NotFound("Matrícula no encontrada.");

        // 2. Validar reglas de negocio (Pagado y Aprobado) - BYPASS para Administradores
        var esAdmin = User.IsInRole("Admin");
        if (!esAdmin && (!matricula.Aprobado || matricula.TotalPagado < matricula.CostoCurso))
        {
            return BadRequest("El alumno no cumple con los requisitos para descargar el certificado (debe estar aprobado y haber pagado el costo completo)");
        }

        try
        {
            // 3. Obtener la plantilla activa (la primera disponible)
            var plantillas = await _seguridadRepository.ListarPlantillasAsync();
            var plantilla = plantillas.FirstOrDefault();
            if (plantilla == null)
                return NotFound("No hay plantillas disponibles");

            // 4. Generar la imagen del certificado
            var detalles = (await _seguridadRepository.ListarDetallesPlantillaAsync(plantilla.IdPlantilla)).ToList();
            byte[] imagenCertificado;

            if (detalles.Count > 0)
            {
                imagenCertificado = _certificadoService.GenerarImagenCertificadoMulticapa(
                    plantilla.RutaImagen,
                    matricula.NombreAlumno,
                    detalles
                );
            }
            else
            {
                imagenCertificado = _certificadoService.GenerarImagenCertificado(
                    plantilla.RutaImagen,
                    matricula.NombreAlumno,
                    plantilla.EjeX,
                    plantilla.EjeY,
                    plantilla.FontSize,
                    plantilla.FontColor
                );
            }

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
