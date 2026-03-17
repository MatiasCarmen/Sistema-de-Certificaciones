namespace Inkillay.Certificados.Web.Models.ViewModels;

public class EditarUsuarioViewModel
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public int IdRol { get; set; }
    public bool Estado { get; set; }
}
