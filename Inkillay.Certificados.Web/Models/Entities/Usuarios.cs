namespace Inkillay.Certificados.Web.Models.Entities;

public class Usuarios
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Clave { get; set; } = string.Empty;
    public int IdRol { get; set; }
    public bool Estado { get; set; }
    public DateTime FechaRegistro { get; set; }
}
