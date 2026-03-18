using SIGEC.Certificados.Web.Data.Repositories;
using SIGEC.Certificados.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SIGEC.Certificados.Web.Controllers;

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
    public IActionResult Registrar(int idModulo)
    {
        ViewBag.IdModulo = idModulo;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(int idModulo, decimal monto, string referencia, string formaPago, string tipoPago)
    {
        var nombreUsuario = User.Identity?.Name ?? "Sistema";
        if (idModulo <= 0 || monto <= 0 || string.IsNullOrWhiteSpace(referencia))
        {
            return Json(new { success = false, message = "Datos de pago invalidos." });
        }

        var ok = await _pagoRepository.RegistrarPagoAsync(
            idModulo,
            monto,
            referencia.Trim(),
            formaPago: "Efectivo",
            tipoPago: "Contado",
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
                $"Matrícula #{idModulo} - Monto: S/ {monto:F2} - Ref: {referencia}",
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
