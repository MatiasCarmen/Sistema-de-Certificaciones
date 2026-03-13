namespace Inkillay.Certificados.Web.Models.Entities;

/// <summary>
/// Clase base abstracta que implementa el estándar corporativo de auditoría.
/// Todas las entidades de negocio deben heredar de esta clase.
/// </summary>
public abstract class AuditoriaBase
{
    /// <summary>
    /// Usuario que registró el registro (nombre desde Claims)
    /// </summary>
    public string? UsuarioRegistro { get; set; }

    /// <summary>
    /// Fecha y hora de registro (se asigna automáticamente por SQL Server)
    /// </summary>
    public DateTime? FechaRegistro { get; set; }

    /// <summary>
    /// Usuario que realizó la última modificación (nombre desde Claims)
    /// </summary>
    public string? UsuarioModifica { get; set; }

    /// <summary>
    /// Fecha y hora de última modificación
    /// </summary>
    public DateTime? FechaModifica { get; set; }
}

/// <summary>
/// Clase para la tabla de auditoría de eventos (tracking de acciones del sistema)
/// NOTA: Esta NO hereda de AuditoriaBase porque es la tabla que registra eventos,
/// no una entidad de negocio auditada.
/// </summary>
public class AuditoriaEvento
{
    public int IdAuditoria { get; set; }
    public int? IdUsuario { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string Modulo { get; set; } = string.Empty;
    public string Detalles { get; set; } = string.Empty;
    public string DireccionIp { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; } = DateTime.Now;
    public bool Exitoso { get; set; } = true;
}
