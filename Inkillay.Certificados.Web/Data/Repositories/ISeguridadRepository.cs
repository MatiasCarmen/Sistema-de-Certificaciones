using Inkillay.Certificados.Web.Models;
using Inkillay.Certificados.Web.Models.Entities;
using Inkillay.Certificados.Web.Models.ViewModels;

namespace Inkillay.Certificados.Web.Data.Repositories;

public interface ISeguridadRepository
{
    Task<IEnumerable<Seg_Modulo>> ListarModulosAsync();
    Task<Usuarios?> ValidarUsuarioAsync(string correo);
    Task<Usuarios> ObtenerUsuarioPorCorreoAsync(string correo);
    Task<IEnumerable<RecentActivityViewModel>> ListarActividadRecienteAsync();
    Task<IEnumerable<Plantilla>> ListarPlantillasAsync();
    Task<int> InsertarPlantillaAsync(Plantilla plantilla);
    Task<int> ActualizarCoordenadasAsync(int id, int x, int y);
    Task<int> ActualizarDisenoPlantillaAsync(int id, int x, int y, int fontSize, string fontColor);
    Task<int> CambiarEstadoPlantillaAsync(int id);

    // Gestión de Usuarios
    Task<IEnumerable<UsuarioViewModel>> ListarUsuariosAsync();
    Task<Usuarios?> ObtenerUsuarioPorIdAsync(int idUsuario);
    Task<bool> RegistrarUsuarioAsync(Usuarios usuario);
    Task<bool> CorreoExisteAsync(string correo);
    Task<int> CambiarEstadoUsuarioAsync(int idUsuario, bool estado);

    // Dashboard Admin
    Task<AdminDashboardViewModel> ObtenerEstadisticasDashboardAsync();
}
