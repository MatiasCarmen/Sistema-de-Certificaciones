namespace Inkillay.Certificados.Web.Data.Repositories;

public interface IPagoRepository
{
    /// <summary>
    /// Registra un pago con auditoría y nuevos campos de negocio
    /// </summary>
    /// <param name="idMatricula">ID de la matrícula</param>
    /// <param name="monto">Monto del pago</param>
    /// <param name="referencia">Referencia del pago</param>
    /// <param name="formaPago">Forma de pago (Efectivo, Tarjeta, etc.)</param>
    /// <param name="tipoPago">Tipo de pago (Contado, Fraccionado, etc.)</param>
    /// <param name="usuarioRegistro">Usuario que registra (para auditoría)</param>
    Task<bool> RegistrarPagoAsync(int idMatricula, decimal monto, string referencia, 
        string formaPago = "Efectivo", string tipoPago = "Contado", string usuarioRegistro = "Sistema");
}
