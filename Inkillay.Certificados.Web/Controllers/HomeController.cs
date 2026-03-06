using Inkillay.Certificados.Web.Data.Repositories;
using Inkillay.Certificados.Web.Models;
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
        var actividad = await _seguridadRepository.ListarActividadRecienteAsync();
        return View(actividad);
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
