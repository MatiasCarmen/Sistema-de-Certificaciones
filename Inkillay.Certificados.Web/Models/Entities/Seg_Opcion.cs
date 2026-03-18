namespace SIGEC.Certificados.Web.Models.Entities;

/// <summary>
/// Entidad Opción de Seguridad según el estándar corporativo actualizado
/// </summary>
public class Seg_Opcion : AuditoriaBase
{
    public int IdOpcion { get; set; }
    public int IdModulo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    
    /// <summary>
    /// Estado de la opción: 'A' = Activo, 'I' = Inactivo
    /// </summary>
    public char Estado { get; set; } = 'A';
    
    // Propiedad auxiliar para compatibilidad
    public bool EstadoActivo => Estado == 'A';
}
