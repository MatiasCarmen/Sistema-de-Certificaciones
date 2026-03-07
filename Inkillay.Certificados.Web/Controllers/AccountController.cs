using Inkillay.Certificados.Web.Data.Repositories;
using Inkillay.Certificados.Web.Models;
using Inkillay.Certificados.Web.Services;
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
                "Admin" => Url.Action("Index", "Plantillas"),
                "Docente" => Url.Action("Index", "Docentes"),
                "Alumno" => Url.Action("MisCursos", "Alumnos"),
                _ => Url.Action("Index", "Home")
            };

            return Json(new { success = true, redirectUrl });
        }

        return Json(new { success = false, message = "Correo o contrasena incorrectos" });
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
