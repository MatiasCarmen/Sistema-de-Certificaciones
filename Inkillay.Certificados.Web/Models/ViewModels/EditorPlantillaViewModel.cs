using Inkillay.Certificados.Web.Models.Entities;

namespace Inkillay.Certificados.Web.Models.ViewModels;

public class EditorPlantillaViewModel
{
    public Plantilla Plantilla { get; set; } = null!;
    public List<PlantillaDetalleDTO> Detalles { get; set; } = new();
}
