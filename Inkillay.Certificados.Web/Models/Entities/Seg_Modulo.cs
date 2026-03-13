namespace Inkillay.Certificados.Web.Models.Entities;

/// <summary>
/// Entidad Módulo de Seguridad según el estándar corporativo actualizado
/// </summary>
public class Seg_Modulo : AuditoriaBase
{
    public int IdModulo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Icono { get; set; }

    /// <summary>
    /// Estado del módulo: 'A' = Activo, 'I' = Inactivo
    /// ⚠️ MIGRACIÓN: Antes era bool, ahora es char(1)
    /// </summary>
    public char Estado { get; set; } = 'A';

    // Propiedad auxiliar para compatibilidad con código legacy
    public bool EstadoActivo => Estado == 'A';
}