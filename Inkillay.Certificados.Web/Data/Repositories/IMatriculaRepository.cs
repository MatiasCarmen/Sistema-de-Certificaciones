using Inkillay.Certificados.Web.Models.Entities;

namespace Inkillay.Certificados.Web.Data.Repositories;

public interface IMatriculaRepository
{
    Task<IEnumerable<Matricula>> ListarAlumnosPorCursoAsync(int idCurso);
    Task<IEnumerable<Matricula>> ListarCursosPorAlumnoAsync(int idAlumno);
}
