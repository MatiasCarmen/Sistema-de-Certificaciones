using Inkillay.Certificados.Web.Data.Repositories;
using Inkillay.Certificados.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inkillay.Certificados.Web.Controllers;

[Authorize(Roles = "Admin,Docente")]
public class PagosController : Controller
{
    private readonly IPagoRepository _pagoRepository;
    private readonly AuditoriaService _auditoriaService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PagosController(IPagoRepository pagoRepository, AuditoriaService auditoriaService, IHttpContextAccessor httpContextAccessor)
    {
        _pagoRepository = pagoRepository;
        _auditoriaService = auditoriaService;
        _httpContextAccessor = httpContextAccessor;
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

        // ✅ AUDITORÍA: Capturar usuario actual
        var nombreUsuario = User.Identity?.Name ?? "Sistema";

        var ok = await _pagoRepository.RegistrarPagoAsync(
            idMatricula, 
            monto, 
            referencia.Trim(),
            formaPago: "Efectivo",  // Por defecto, puede parametrizarse en el futuro
            tipoPago: "Contado",     // Por defecto, puede parametrizarse en el futuro
            usuarioRegistro: nombreUsuario
        );

        // Registrar auditoría
        var idUsuario = User.FindFirst("IdUsuario");
        var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "DESCONOCIDA";

        if (ok)
        {
            await _auditoriaService.RegistrarAsync(
                idUsuario != null ? int.Parse(idUsuario.Value) : null,
                "REGISTRO PAGO",
                "Pagos",
                $"Matrícula #{idMatricula} - Monto: S/ {monto:F2} - Ref: {referencia}",
                ip
            );
        }

        return Json(new
        {
            success = ok,
            message = ok ? "Pago registrado correctamente." : "No se pudo registrar el pago."
        });
    }
}

