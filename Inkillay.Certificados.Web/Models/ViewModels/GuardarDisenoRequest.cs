namespace Inkillay.Certificados.Web.Models.ViewModels;

public class GuardarDisenoRequest
{
    public int id { get; set; }
    public int ejeX { get; set; }
    public int ejeY { get; set; }
    public int fontSize { get; set; }
    public string fontColor { get; set; } = "#000000";
    public bool estado { get; set; }
}
