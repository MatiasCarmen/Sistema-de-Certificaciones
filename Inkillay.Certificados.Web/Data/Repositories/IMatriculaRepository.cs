using Inkillay.Certificados.Web.Models.Entities;

namespace Inkillay.Certificados.Web.Data.Repositories;

public interface IMatriculaRepository
{
    Task<IEnumerable<Matricula>> ListarAlumnosPorCursoAsync(int idCurso);
    Task<IEnumerable<Matricula>> ListarCursosPorAlumnoAsync(int idAlumno);

    /// <summary>
    /// Registra una nueva matrícula con auditoría
    /// </summary>
    /// <param name="idAlumno">ID del alumno</param>
    /// <param name="idCurso">ID del curso</param>
    /// <param name="usuarioRegistro">Usuario que registra (para auditoría)</param>
    Task<bool> RegistrarMatriculaAsync(int idAlumno, int idCurso, string usuarioRegistro = "Sistema");
}
