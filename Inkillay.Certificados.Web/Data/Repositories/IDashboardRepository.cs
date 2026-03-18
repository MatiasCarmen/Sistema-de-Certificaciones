using SIGEC.Certificados.Web.Models.ViewModels;

namespace SIGEC.Certificados.Web.Data.Repositories;

public interface IDashboardRepository
{
    Task<AdminDashboardViewModel?> ObtenerEstadisticasDashboardAsync();
    Task<IEnumerable<ActividadReciente>> ListarActividadRecienteAsync();
    Task<DocenteDashboardViewModel?> ObtenerStatsDocenteAsync(int idDocente);
    Task<AlumnoDashboardViewModel?> ObtenerStatsAlumnoAsync(int idUsuario);
}
