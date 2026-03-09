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
    private readonly ISeguridadRepository _seguridadRepository;
    private readonly AuditoriaService _auditoriaService;

    public HomeController(ILogger<HomeController> logger, ISeguridadRepository seguridadRepository, AuditoriaService auditoriaService)
    {
        _logger = logger;
        _seguridadRepository = seguridadRepository;
        _auditoriaService = auditoriaService;
    }

    public async Task<IActionResult> Index()
    {
        // Si es Admin, mostrar dashboard
        if (User.IsInRole("Admin"))
        {
            try
            {
                var dashboard = await _seguridadRepository.ObtenerEstadisticasDashboardAsync();

                // Obtener actividad reciente
                var actividad = await _auditoriaService.ObtenerUltimosAsync(5);
                dashboard.ActividadReciente = actividad.Select(a => new ActividadReciente
                {
                    Accion = a.accion,
                    Detalles = a.detalles,
                    Fecha = a.fecha
                }).ToList();

                return View("AdminDashboard", dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar estadísticas del dashboard");
                var actividad = await _seguridadRepository.ListarActividadRecienteAsync();
                return View("Index", actividad);
            }
        }

        // Si es Docente o Alumno, mostrar actividad reciente
        var recentActivity = await _seguridadRepository.ListarActividadRecienteAsync();
        return View(recentActivity);
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
