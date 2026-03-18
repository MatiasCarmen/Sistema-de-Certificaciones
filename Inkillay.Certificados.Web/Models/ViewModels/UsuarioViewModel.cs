namespace SIGEC.Certificados.Web.Models.ViewModels;

public class UsuarioViewModel
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string NombreRol { get; set; } = string.Empty;
    public int IdRol { get; set; }
    public bool Estado { get; set; }
    public DateTime FechaRegistro { get; set; }
}
