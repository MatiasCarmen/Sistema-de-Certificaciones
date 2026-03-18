using SIGEC.Certificados.Web.Models.Entities;

namespace SIGEC.Certificados.Web.Data.Repositories;

public interface IModuloRepository
{
    // Módulos (Cabecera)
    Task<IEnumerable<Modulo>> ListarTodosAsync();
    Task<Modulo?> ObtenerPorIdAsync(int idModulo);
    Task<IEnumerable<Modulo>> ListarPorCursoAsync(int idCurso);
    Task<int> RegistrarModuloAsync(Modulo modulo);
    Task<bool> ActualizarModuloAsync(Modulo modulo);
    Task<bool> EliminarModuloAsync(int idModulo);
}
