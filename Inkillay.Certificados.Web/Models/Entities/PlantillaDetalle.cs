namespace Inkillay.Certificados.Web.Models.Entities;

/// <summary>
/// Entidad PlantillaDetalle - Detalle de capas de texto en certificados multicapa
/// </summary>
public class PlantillaDetalle : AuditoriaBase
{
    public int IdDetalle { get; set; }
    public int IdPlantilla { get; set; }
    public string Texto { get; set; } = string.Empty;
    public int EjeX { get; set; } = 0;
    public int EjeY { get; set; } = 0;
    public int FontSize { get; set; } = 40;
    public string FontColor { get; set; } = "#000000";
    
    /// <summary>
    /// Indica si es la capa principal (nombre del alumno)
    /// </summary>
    public bool EsPrincipal { get; set; } = false;
    
    /// <summary>
    /// Orden de renderizado de las capas
    /// </summary>
    public int Orden { get; set; } = 0;
}
