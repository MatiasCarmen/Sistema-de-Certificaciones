namespace Inkillay.Certificados.Web.Models.Entities;

public class Auditoria
{
    public int IdAuditoria { get; set; }
    public int? IdUsuario { get; set; }
    public string Accion { get; set; } // "EMISIÓN PDF", "REGISTRO PAGO", etc
    public string Modulo { get; set; } // "Certificados", "Pagos", "Usuarios"
    public string Detalles { get; set; }
    public string DireccionIp { get; set; }
    public DateTime FechaHora { get; set; } = DateTime.Now;
    public bool Exitoso { get; set; } = true;
}
