using Inkillay.Certificados.Web.Models.Entities;

namespace Inkillay.Certificados.Web.Data.Repositories;

public interface IModuloRepository
{
    // Módulos (Cabecera)
    Task<IEnumerable<Modulo>> ListarTodosAsync();
    Task<Modulo?> ObtenerPorIdAsync(int idModulo);
    Task<IEnumerable<Modulo>> ListarPorCursoAsync(int idCurso);
    Task<int> RegistrarModuloAsync(Modulo modulo);
    Task<bool> ActualizarModuloAsync(Modulo modulo);

    // Legacy Support para Vistas Existentes (Alumnos, Emisión, Docentes)
    Task<IEnumerable<Modulo>> ListarAlumnosPorCursoAsync(int idCurso);
    Task<IEnumerable<Modulo>> ListarCursosPorAlumnoAsync(int idUsuario);
    Task<bool> RegistrarModuloAsync(int idAlumno, int idCurso, char modalidad, string usuarioRegistro);

    // Módulo Alumnos (Detalle)
    Task<IEnumerable<ModuloAlumno>> ListarAlumnosPorModuloAsync(int idModulo);
    Task<IEnumerable<ModuloAlumno>> ListarModulosPorAlumnoAsync(int idUsuario);
    Task<bool> RegistrarModuloAlumnoAsync(ModuloAlumno moduloAlumno);
    Task<bool> ActualizarNotaFinalAsync(int idModuloAlumno, decimal notaFinal, string usuarioModifica);
}
