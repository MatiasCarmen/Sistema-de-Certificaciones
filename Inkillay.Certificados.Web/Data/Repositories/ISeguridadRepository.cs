using Inkillay.Certificados.Web.Models;
using Inkillay.Certificados.Web.Models.Entities;
using Inkillay.Certificados.Web.Models.ViewModels;

namespace Inkillay.Certificados.Web.Data.Repositories;

public interface ISeguridadRepository
{
    // Módulos
    Task<IEnumerable<Seg_Modulo>> ListarModulosAsync();

    // Usuarios
    Task<Usuarios?> ValidarUsuarioAsync(string correo);
    Task<Usuarios> ObtenerUsuarioPorCorreoAsync(string correo);
    Task<Usuarios?> ObtenerUsuarioPorIdAsync(int id);
    Task<IEnumerable<Usuarios>> ListarUsuariosAsync();
    Task<bool> CorreoExisteAsync(string correo);
    Task<bool> RegistrarUsuarioAsync(Usuarios usuario);
    Task<int> CambiarEstadoUsuarioAsync(int id, bool estado);
    Task<AdminDashboardViewModel?> ObtenerEstadisticasDashboardAsync();

    // Actividad Reciente
    Task<IEnumerable<RecentActivityViewModel>> ListarActividadRecienteAsync();

    // Plantillas
    Task<IEnumerable<Plantilla>> ListarPlantillasAsync();
    Task<int> InsertarPlantillaAsync(Plantilla plantilla);
    Task<int> ActualizarCoordenadasAsync(int id, int x, int y);
    Task<int> ActualizarDisenoPlantillaAsync(int id, int x, int y, int fontSize, string fontColor);
    Task<bool> GuardarDisenoCompletoAsync(int idPlantilla, List<PlantillaDetalleDTO> detalles);
    Task<IEnumerable<PlantillaDetalleDTO>> ListarDetallesPlantillaAsync(int idPlantilla);
    Task<int> CambiarEstadoPlantillaAsync(int id);
}
