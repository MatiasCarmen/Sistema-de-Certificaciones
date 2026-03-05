namespace Inkillay.Certificados.Web.Models.Entities;

public class Seg_Modulo
{
    public int IdModulo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Icono { get; set; }
    public bool Estado { get; set; }
}