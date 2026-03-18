using SIGEC.Certificados.Web.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SIGEC.Certificados.Web.Controllers;

[Authorize(Roles = "Admin")]
public class SeguridadController : Controller
{
    private readonly ISeguridadRepository _seguridadRepository;

    // El sistema nos entrega automaticamente el repositorio gracias al AddScoped que hiciste
    public SeguridadController(ISeguridadRepository seguridadRepository)
    {
        _seguridadRepository = seguridadRepository;
    }

    public async Task<IActionResult> Modulos()
    {
        // Llamamos al repositorio para obtener la lista de la DB
        var modulos = await _seguridadRepository.ListarModulosAsync();

        // Pasamos los datos a la Vista
        return View(modulos);
    }
}
