using SIGEC.Certificados.Web.Data.Repositories;
using SIGEC.Certificados.Web.Models;
using SIGEC.Certificados.Web.Models.ViewModels;
using SIGEC.Certificados.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace SIGEC.Certificados.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IDashboardRepository _dashboardRepository;
    private readonly AuditoriaService _auditoriaService;

    public HomeController(ILogger<HomeController> logger, IDashboardRepository dashboardRepository, AuditoriaService auditoriaService)
    {
        _logger = logger;
        _dashboardRepository = dashboardRepository;
        _auditoriaService = auditoriaService;
    }

    public async Task<IActionResult> Index()
    {
        if (User.IsInRole("Admin"))
            return RedirectToAction("AdminDashboard");

        if (User.IsInRole("Docente"))
            return RedirectToAction("DocenteDashboard");

        if (User.IsInRole("Alumno"))
            return RedirectToAction("AlumnoDashboard");

        return RedirectToAction("AdminDashboard");
    }

    // ─── ADMIN ───────────────────────────────────────────────────────────────
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminDashboard()
    {
        var dashboard = new AdminDashboardViewModel();

        try
        {
            var data = await _dashboardRepository.ObtenerEstadisticasDashboardAsync();
            if (data != null)
            {
                dashboard.TotalAlumnos        = data.TotalAlumnos;
                dashboard.CursosActivos       = data.CursosActivos;
                dashboard.RecaudacionTotal    = data.RecaudacionTotal;
                dashboard.CertificadosEmitidos = data.CertificadosEmitidos;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudieron cargar estadísticas de dashboard.");
        }

        try
        {
            var actividad = await _auditoriaService.ObtenerUltimosAsync(5);
            dashboard.ActividadReciente = actividad?
                .Select(a => new ActividadReciente
                {
                    Accion   = a.accion,
                    Detalles = a.detalles,
                    Fecha    = a.fecha
                }).ToList() ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo cargar actividad reciente.");
            dashboard.ActividadReciente = new();
        }

        dashboard.GraficoRecaudacion ??= new();

        return View("AdminDashboard", dashboard);
    }

    // ─── DOCENTE ─────────────────────────────────────────────────────────────
    [Authorize(Roles = "Admin,Docente")]
    public async Task<IActionResult> DocenteDashboard()
    {
        var idUsuarioClaim = User.FindFirst("IdUsuario")?.Value;
        var vm = new DocenteDashboardViewModel();
        if (int.TryParse(idUsuarioClaim, out int idDocente))
        {
            try { vm = await _dashboardRepository.ObtenerStatsDocenteAsync(idDocente) ?? vm; }
            catch (Exception ex) { _logger.LogWarning(ex, "Stats docente no disponibles."); }
        }
        return View("DocenteDashboard", vm);
    }

    // ─── ALUMNO ──────────────────────────────────────────────────────────────
    [Authorize(Roles = "Admin,Alumno")]
    public async Task<IActionResult> AlumnoDashboard()
    {
        var idUsuarioClaim = User.FindFirst("IdUsuario")?.Value;
        var vm = new AlumnoDashboardViewModel();
        if (int.TryParse(idUsuarioClaim, out int idUsuario))
        {
            try { vm = await _dashboardRepository.ObtenerStatsAlumnoAsync(idUsuario) ?? vm; }
            catch (Exception ex) { _logger.LogWarning(ex, "Stats alumno no disponibles."); }
        }
        return View("AlumnoDashboard", vm);
    }

    // ─── UTILS ───────────────────────────────────────────────────────────────
    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
