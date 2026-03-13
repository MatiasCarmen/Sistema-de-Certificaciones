namespace Inkillay.Certificados.Web.Models.Entities;

/// <summary>
/// Entidad Pago según el estándar corporativo actualizado
/// </summary>
public class Pago : AuditoriaBase
{
    public int IdPago { get; set; }
    public int IdMatricula { get; set; }
    public decimal Monto { get; set; }
    public DateTime? FechaPago { get; set; }
    public string? Referencia { get; set; }
    
    /// <summary>
    /// Forma de pago: Efectivo, Tarjeta, Transferencia, Yape, Plin
    /// ⚠️ NUEVO CAMPO según estándar corporativo
    /// </summary>
    public string? FormaPago { get; set; }
    
    /// <summary>
    /// Tipo de pago: Contado, Fraccionado, Cuota
    /// ⚠️ NUEVO CAMPO según estándar corporativo
    /// </summary>
    public string? TipoPago { get; set; }
}
