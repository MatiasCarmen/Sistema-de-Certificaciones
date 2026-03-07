namespace Inkillay.Certificados.Web.Data.Repositories;

public interface IPagoRepository
{
    Task<bool> RegistrarPagoAsync(int idMatricula, decimal monto, string referencia);
}
