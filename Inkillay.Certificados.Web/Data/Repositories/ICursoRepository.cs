using SIGEC.Certificados.Web.Models.ViewModels;
using SIGEC.Certificados.Web.Models.Entities;

namespace SIGEC.Certificados.Web.Data.Repositories;

public interface ICursoRepository
{
    Task<IEnumerable<Curso>> ListarTodosAsync();
    Task<IEnumerable<Curso>> ListarCursosActivosAsync();
    Task<Curso?> ObtenerPorIdAsync(int idCurso);
    Task<IEnumerable<ReporteCursoViewModel>> ObtenerReporteDocenteAsync(int idProfesor);
    Task<int> RegistrarCursoAsync(Curso curso);
    Task<bool> ActualizarCursoAsync(Curso curso);
}
