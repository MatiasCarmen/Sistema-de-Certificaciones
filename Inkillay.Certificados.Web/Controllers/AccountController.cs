using Inkillay.Certificados.Web.Data.Repositories;
using Inkillay.Certificados.Web.Models;
using Inkillay.Certificados.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace Inkillay.Certificados.Web.Controllers;

public class AccountController : Controller
{
    private readonly ISeguridadRepository _seguridadRepository;
    private readonly ILogger<AccountController> _logger;

    public AccountController(ISeguridadRepository seguridadRepository, ILogger<AccountController> logger)
    {
        _seguridadRepository = seguridadRepository;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [EnableRateLimiting("login-policy")]  // 🛡️ Aplica protección contra fuerza bruta
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        try
        {
            _logger.LogInformation("Intento de login para el correo: {Correo}", model.Correo);

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Datos invalidos" });
            }

            var usuario = await _seguridadRepository.ObtenerUsuarioPorCorreoAsync(model.Correo);

            if (usuario != null && HashHelper.VerifyPassword(model.Clave, usuario.Clave))
            {
                var roleName = ResolveRoleName(usuario.IdRol);
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.Nombre),
                    new Claim(ClaimTypes.Email, usuario.Correo),
                    new Claim("IdUsuario", usuario.IdUsuario.ToString()),
                    new Claim("IdRol", usuario.IdRol.ToString()),
                    new Claim(ClaimTypes.Role, roleName)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                var redirectUrl = roleName switch
                {
                    "Admin" => Url.Action("AdminDashboard", "Home"),
                    "Docente" => Url.Action("Index", "Docentes"),
                    "Alumno" => Url.Action("MisCursos", "Alumnos"),
                    _ => Url.Action("Index", "Home")
                };

                return Json(new { success = true, redirectUrl });
            }

            _logger.LogWarning("Login fallido para {Correo}", model.Correo);
            return Json(new { success = false, message = "Correo o contrasena incorrectos" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error critico en el proceso de Login para {Correo}", model.Correo);
            return Json(new { success = false, message = "Error interno del servidor" });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    private static string ResolveRoleName(int idRol)
    {
        return idRol switch
        {
            1 => "Admin",
            2 => "Docente",
            3 => "Alumno",
            _ => "Alumno"
        };
    }
}
