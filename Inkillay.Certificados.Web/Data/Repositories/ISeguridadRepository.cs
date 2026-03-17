using Inkillay.Certificados.Web.Models;
using Inkillay.Certificados.Web.Models.Entities;
using Inkillay.Certificados.Web.Models.ViewModels;

namespace Inkillay.Certificados.Web.Data.Repositories;

public interface ISeguridadRepository
{
    // Módulos de Seguridad
    Task<IEnumerable<Seg_Modulo>> ListarModulosAsync();

    // Usuarios
    Task<Usuarios?> ValidarUsuarioAsync(string correo);
    Task<Usuarios> ObtenerUsuarioPorCorreoAsync(string correo);
    Task<Usuarios?> ObtenerUsuarioPorIdAsync(int id);

    // --- AGREGAR ESTA LÍNEA PARA EL CONTROLADOR DE MÓDULOS ---
    Task<IEnumerable<Usuarios>> ListarTodosAsync();

    Task<IEnumerable<Usuarios>> ListarUsuariosAsync();
    Task<bool> CorreoExisteAsync(string correo);
    Task<bool> RegistrarUsuarioAsync(Usuarios usuario);
    Task<bool> RegistrarAlumnoCompletoAsync(CrearUsuarioViewModel modelo, string usuarioRegistro);
    Task<EditarAlumnoViewModel> ObtenerAlumnoPorIdAsync(int idUsuario);
    Task<bool> ActualizarAlumnoCompletoAsync(EditarAlumnoViewModel modelo, string usuarioModifica);
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