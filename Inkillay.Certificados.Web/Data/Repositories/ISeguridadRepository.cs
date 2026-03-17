using Inkillay.Certificados.Web.Models.Entities;
using Inkillay.Certificados.Web.Models.ViewModels;

namespace Inkillay.Certificados.Web.Data.Repositories;

public interface ISeguridadRepository
{
    // Usuarios
    Task<IEnumerable<Usuarios>> ListarUsuariosAsync();
    Task<Usuarios?> ObtenerUsuarioPorIdAsync(int idUsuario);
    Task<bool> CorreoExisteAsync(string correo);
    Task<bool> RegistrarUsuarioAsync(Usuarios usuario);
    Task<int> CambiarEstadoUsuarioAsync(int idUsuario, bool estado);

    // Alumnos (Perfil extendido)
    Task<EditarAlumnoViewModel?> ObtenerAlumnoPorIdAsync(int idAlumno);
    Task<bool> ActualizarAlumnoCompletoAsync(EditarAlumnoViewModel model, string usuarioModifica);
    Task<bool> RegistrarAlumnoCompletoAsync(CrearUsuarioViewModel model, string usuarioAdmin);

    // Roles y Modulos de Seguridad
    Task<IEnumerable<Rol>> ListarRolesAsync();
    Task<IEnumerable<Seg_Modulo>> ListarSegModulosAsync();
}