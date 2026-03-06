using Inkillay.Certificados.Web.Data.Repositories;
using Inkillay.Certificados.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Inkillay.Certificados.Web.Controllers;

public class AccountController : Controller
{
    private readonly ISeguridadRepository _seguridadRepository;

    public AccountController(ISeguridadRepository seguridadRepository)
    {
        _seguridadRepository = seguridadRepository;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return Json(new { success = false, message = "Datos inválidos" });

        // 1. Buscamos al usuario en la DB
        // Nota: Necesitaremos agregar este método a nuestro repositorio
        var usuario = await _seguridadRepository.ValidarUsuarioAsync(model.Correo);

        if (usuario != null && usuario.Clave == model.Clave) // ¡Aquí compararemos el Hash luego!
        {
            // 2. Si es correcto, creamos la identidad del usuario
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim("IdUsuario", usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Role, usuario.IdRol.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
        }

        return Json(new { success = false, message = "Correo o contraseña incorrectos" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }
}
