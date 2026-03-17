using Inkillay.Certificados.Web.Models.ViewModels;

namespace Inkillay.Certificados.Web.Data.Repositories;

public interface IDashboardRepository
{
    Task<AdminDashboardViewModel?> ObtenerEstadisticasDashboardAsync();
    Task<IEnumerable<ActividadReciente>> ListarActividadRecienteAsync();
}
