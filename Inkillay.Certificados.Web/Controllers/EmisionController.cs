using SIGEC.Certificados.Web.Data.Repositories;
using SIGEC.Certificados.Web.Models.Entities;
using SIGEC.Certificados.Web.Models.ViewModels;
using SIGEC.Certificados.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SIGEC.Certificados.Web.Controllers;

[Authorize(Roles = "Docente,Admin")]
public class EmisionController : Controller
{
    private readonly ICursoRepository _cursoRepository;
    private readonly IModuloRepository _moduloRepository;
    private readonly IMatriculaRepository _matriculaRepository;
    private readonly IPlantillaRepository _plantillaRepository;
    private readonly ICertificadoService _certificadoService;
    private readonly AuditoriaService _auditoriaService;
    private readonly ILogger<EmisionController> _logger;

    public EmisionController(
        ICursoRepository cursoRepository,
        IModuloRepository moduloRepository,
        IMatriculaRepository matriculaRepository,
        IPlantillaRepository plantillaRepository,
        ICertificadoService certificadoService,
        AuditoriaService auditoriaService,
        ILogger<EmisionController> logger)
    {
        _cursoRepository = cursoRepository;
        _moduloRepository = moduloRepository;
        _matriculaRepository = matriculaRepository;
        _plantillaRepository = plantillaRepository;
        _certificadoService = certificadoService;
        _auditoriaService = auditoriaService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? idCurso)
    {
        var cursos = await _cursoRepository.ListarCursosActivosAsync();
        var plantillas = await _plantillaRepository.ListarPlantillasAsync();
        
        // Usando Matricula en lugar de Modulo para los alumnos
        var alumnos = idCurso.HasValue
            ? await _matriculaRepository.ListarAlumnosPorModuloAsync(idCurso.Value)
            : Enumerable.Empty<Matricula>();

        var vm = new EmisionIndexViewModel
        {
            Cursos = cursos,
            Plantillas = plantillas,
            Alumnos = alumnos,
            IdCursoSeleccionado = idCurso
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> DescargarPdf(int idModulo, int idPlantilla)
    {
        try
        {
            // 1. Obtener la matrícula (usando el nuevo SP que creaste)
            var matricula = await _matriculaRepository.ObtenerPorIdAsync(idModulo);
            if (matricula == null)
                return NotFound("No se encontró la matrícula.");

            // 2. Obtener la plantilla
            var plantillas = await _plantillaRepository.ListarPlantillasAsync();
            var plantilla = idPlantilla > 0
                ? plantillas.FirstOrDefault(x => x.IdPlantilla == idPlantilla)
                : plantillas.FirstOrDefault();

            if (plantilla == null)
                return NotFound("No hay plantillas disponibles.");

            // 3. Obtener los detalles de la plantilla
            var detalles = (await _plantillaRepository.ListarDetallesPlantillaAsync(plantilla.IdPlantilla)).ToList();

            // 4. Generar el PDF
            byte[] imagenBytes;
            if (detalles.Count > 0)
            {
                imagenBytes = _certificadoService.GenerarImagenCertificadoMulticapa(
                    plantilla.RutaImagen,
                    matricula.NombreAlumno,
                    detalles
                );
            }
            else
            {
                // Proveer valores por defecto de tamaño de fuente y color si son nulos en el objeto (asumiendo que EjeX y EjeY existen)
                imagenBytes = _certificadoService.GenerarImagenCertificado(
                    plantilla.RutaImagen,
                    matricula.NombreAlumno,
                    plantilla.EjeX,
                    plantilla.EjeY,
                    plantilla.FontSize,
                    plantilla.FontColor ?? "#000000"
                );
            }

            byte[] pdfBytes = _certificadoService.GenerarPdfDesdeImagen(imagenBytes);

            string nombreDescarga = $"Certificado_{matricula.NombreAlumno.Replace(" ", "_")}.pdf";
            return File(pdfBytes, "application/pdf", nombreDescarga);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al descargar PDF para matrícula {id}", idModulo);
            return StatusCode(500, $"Error al generar certificado: {ex.Message}");
        }
    }
}
