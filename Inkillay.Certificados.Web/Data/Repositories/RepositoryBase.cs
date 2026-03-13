using System.Security.Claims;

namespace Inkillay.Certificados.Web.Data.Repositories;

/// <summary>
/// Clase base para repositorios que implementa utilidades comunes de auditoría
/// </summary>
public abstract class RepositoryBase
{
    /// <summary>
    /// Extrae el nombre del usuario actual desde los Claims para campos de auditoría
    /// </summary>
    /// <param name="user">ClaimsPrincipal del usuario actual</param>
    /// <returns>Nombre del usuario o "Sistema" si no está autenticado</returns>
    protected string ObtenerUsuarioActual(ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true)
            return "Sistema";
            
        return user.FindFirst(ClaimTypes.Name)?.Value 
            ?? user.FindFirst(ClaimTypes.Email)?.Value 
            ?? "Sistema";
    }
    
    /// <summary>
    /// Convierte un bool a char para el campo Estado ('A' = Activo, 'I' = Inactivo)
    /// </summary>
    protected char ConvertirEstado(bool estado) => estado ? 'A' : 'I';
    
    /// <summary>
    /// Convierte un char a bool desde el campo Estado
    /// </summary>
    protected bool ConvertirEstadoABool(char estado) => estado == 'A';
}
