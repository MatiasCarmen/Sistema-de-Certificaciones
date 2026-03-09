using Inkillay.Certificados.Web.Data.Repositories;
using Inkillay.Certificados.Web.Models;
using Inkillay.Certificados.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Inkillay.Certificados.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ISeguridadRepository _seguridadRepository;

    public HomeController(ILogger<HomeController> logger, ISeguridadRepository seguridadRepository)
    {
        _logger = logger;
        _seguridadRepository = seguridadRepository;
    }

    public async Task<IActionResult> Index()
    {
        // Si es Admin, mostrar dashboard
        if (User.IsInRole("Admin"))
        {
            try
            {
                var dashboard = await _seguridadRepository.ObtenerEstadisticasDashboardAsync();
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
