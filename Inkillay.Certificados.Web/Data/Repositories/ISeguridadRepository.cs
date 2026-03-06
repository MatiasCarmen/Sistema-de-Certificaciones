using Inkillay.Certificados.Web.Models;
using Inkillay.Certificados.Web.Models.Entities;

namespace Inkillay.Certificados.Web.Data.Repositories;

public interface ISeguridadRepository
{
    Task<IEnumerable<Seg_Modulo>> ListarModulosAsync();
    Task<Usuarios?> ValidarUsuarioAsync(string correo);
    Task<IEnumerable<RecentActivityViewModel>> ListarActividadRecienteAsync();
    Task<IEnumerable<Plantilla>> ListarPlantillasAsync();
    Task<int> InsertarPlantillaAsync(Plantilla plantilla);
    Task<int> ActualizarCoordenadasAsync(int id, int x, int y);
    Task<int> CambiarEstadoPlantillaAsync(int id);
}
