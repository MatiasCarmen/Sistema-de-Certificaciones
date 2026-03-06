namespace Inkillay.Certificados.Web.Models.Entities;

public class Plantilla
{
    public int IdPlantilla { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string RutaImagen { get; set; } = string.Empty;
    public int EjeX { get; set; }
    public int EjeY { get; set; }
    public bool Estado { get; set; }
}
