using Inkillay.Certificados.Web.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inkillay.Certificados.Web.Controllers;

[Authorize(Roles = "Admin,Docente")]
public class PagosController : Controller
{
    private readonly IPagoRepository _pagoRepository;

    public PagosController(IPagoRepository pagoRepository)
    {
        _pagoRepository = pagoRepository;
    }

    [HttpGet]
    public IActionResult Registrar(int idMatricula)
    {
        ViewBag.IdMatricula = idMatricula;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(int idMatricula, decimal monto, string referencia)
    {
        if (idMatricula <= 0 || monto <= 0 || string.IsNullOrWhiteSpace(referencia))
        {
            return Json(new { success = false, message = "Datos de pago invalidos." });
        }

        var ok = await _pagoRepository.RegistrarPagoAsync(idMatricula, monto, referencia.Trim());
        return Json(new
        {
            success = ok,
            message = ok ? "Pago registrado correctamente." : "No se pudo registrar el pago."
        });
    }
}
