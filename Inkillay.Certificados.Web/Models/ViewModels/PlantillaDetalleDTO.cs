namespace SIGEC.Certificados.Web.Models.ViewModels;

public class PlantillaDetalleDTO
{
    public string Texto { get; set; } = string.Empty;
    public int X { get; set; }
    public int Y { get; set; }
    public int FontSize { get; set; } = 40;
    public string FontColor { get; set; } = "#000000";
    public int EsPrincipal { get; set; }
    public int Orden { get; set; }
}
