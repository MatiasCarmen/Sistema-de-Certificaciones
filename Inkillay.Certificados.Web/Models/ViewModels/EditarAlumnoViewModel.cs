namespace Inkillay.Certificados.Web.Models.ViewModels;

public class EditarAlumnoViewModel
{
    public int IdUsuario { get; set; }
    public string Correo { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string ApellidoPaterno { get; set; } = string.Empty;
    public string ApellidoMaterno { get; set; } = string.Empty;
    public DateTime? FechaNacimiento { get; set; }
    public string? Ciudad { get; set; }
    public string? Telefono { get; set; }
    public string? TipoDocumento { get; set; }
    public string? NumeroDocumento { get; set; }
    public string? ClaveNueva { get; set; }
    public bool Estado { get; set; }
}
