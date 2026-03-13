namespace Inkillay.Certificados.Web.Models.Entities;

/// <summary>
/// Entidad Usuario según el estándar corporativo actualizado
/// </summary>
public class Usuarios : AuditoriaBase
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Clave { get; set; } = string.Empty;
    public int IdRol { get; set; }

    /// <summary>
    /// Estado del usuario: 'A' = Activo, 'I' = Inactivo
    /// ⚠️ MIGRACIÓN: Antes era bool, ahora es char(1)
    /// </summary>
    public char Estado { get; set; } = 'A';

    // Propiedad auxiliar para compatibilidad con código legacy
    public bool EstadoActivo => Estado == 'A';
}
