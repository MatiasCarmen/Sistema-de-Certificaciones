using Inkillay.Certificados.Web.Data.Repositories;
using Inkillay.Certificados.Web.Models;
using Inkillay.Certificados.Web.Models.ViewModels;
using Inkillay.Certificados.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Inkillay.Certificados.Web.Controllers;

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
        // Administrador: Dashboard completo con métricas globales
        if (User.IsInRole("Admin"))
        {
            return RedirectToAction("AdminDashboard");
        }

        // Docente: Dashboard de gestión de cursos y métricas de alumnos
        if (User.IsInRole("Docente"))
        {
            return RedirectToAction("Index", "Docentes");
        }

        // Alumno: Portal de cursos matriculados y certificados
        if (User.IsInRole("Alumno"))
        {
            return RedirectToAction("MisCursos", "Alumnos");
        }

        // Fallback genérico para roles no definidos (como Staff genérico o invitados validos)
        var recentActivity = await _dashboardRepository.ListarActividadRecienteAsync();
        return View(recentActivity);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminDashboard()
    {
        try
        {
            var dashboard = await _dashboardRepository.ObtenerEstadisticasDashboardAsync();
            if (dashboard == null) dashboard = new AdminDashboardViewModel();

            var actividad = await _auditoriaService.ObtenerUltimosAsync(5);
            dashboard.ActividadReciente = actividad.Select(a => new ActividadReciente
            {
                Accion = a.accion,
                Detalles = a.detalles,
                Fecha = a.fecha
            }).ToList();

            return View(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar estadísticas del dashboard de Admin");
            return RedirectToAction("Error");
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
