namespace Inkillay.Certificados.Web.Models.ViewModels;

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

public class CrearUsuarioViewModel
{
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Clave { get; set; } = string.Empty;
    public int IdRol { get; set; }

    // Nuevos campos exclusivos para Alumno
    public string? ApellidoPaterno { get; set; }
    public string? ApellidoMaterno { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public string? Ciudad { get; set; }
    public string? Telefono { get; set; }
    public string? TipoDocumento { get; set; }
    public string? NumeroDocumento { get; set; }
}

public class EditarUsuarioViewModel
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public int IdRol { get; set; }
    public bool Estado { get; set; }
}

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
    public string? ClaveNueva { get; set; } // Opcional por si quiere resetearla
    public bool Estado { get; set; }
}
