namespace Inkillay.Certificados.Web.Models.ViewModels;

public class CrearUsuarioViewModel
{
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Clave { get; set; } = string.Empty;
    public int IdRol { get; set; }

    // Campos de Alumno
    public string? ApellidoPaterno { get; set; }
    public string? ApellidoMaterno { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public string? Ciudad { get; set; }
    public string? Telefono { get; set; }
    public string? TipoDocumento { get; set; }
    public string? NumeroDocumento { get; set; }
}
