namespace Inkillay.Certificados.Web.Models.Entities;

/// <summary>
/// Entidad Plantilla de certificados según el estándar corporativo actualizado
/// </summary>
public class Plantilla : AuditoriaBase
{
    public int IdPlantilla { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string RutaImagen { get; set; } = string.Empty;
    public int EjeX { get; set; } = 50;
    public int EjeY { get; set; } = 50;
    public int FontSize { get; set; } = 60;
    public string FontColor { get; set; } = "#000000";

    /// <summary>
    /// Familia de fuente para el texto del certificado
    /// ⚠️ NUEVO CAMPO según estándar corporativo
    /// </summary>
    public string FontFamily { get; set; } = "Arial";

    /// <summary>
    /// Estado de la plantilla: 'A' = Activo, 'I' = Inactivo
    /// ⚠️ MIGRACIÓN: Antes era bool, ahora es char(1)
    /// </summary>
    public char Estado { get; set; } = 'A';

    // Propiedad auxiliar para compatibilidad con código legacy
    public bool EstadoActivo => Estado == 'A';
}
