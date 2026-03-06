using Inkillay.Certificados.Web.Models.Entities;

namespace Inkillay.Certificados.Web.Data.Repositories;

public interface ISeguridadRepository
{
    // Esta es la "promesa" de que el repositorio sabrá listar módulos
    Task<IEnumerable<Seg_Modulo>> ListarModulosAsync();

    // Nueva tarea: Buscar un usuario por su correo para el Login
    Task<Usuarios?> ValidarUsuarioAsync(string correo);
}
