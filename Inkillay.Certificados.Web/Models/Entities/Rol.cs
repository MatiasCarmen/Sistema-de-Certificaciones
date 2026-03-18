namespace SIGEC.Certificados.Web.Models.Entities;

/// <summary>
/// Entidad Rol según el estándar corporativo actualizado
/// </summary>
public class Rol : AuditoriaBase
{
    public int IdRol { get; set; }
    public string Nombre { get; set; } = string.Empty;
    
    /// <summary>
    /// Estado del rol: 'A' = Activo, 'I' = Inactivo
    /// </summary>
    public char Estado { get; set; } = 'A';
    
    // Propiedad auxiliar para compatibilidad
    public bool EstadoActivo => Estado == 'A';
}
