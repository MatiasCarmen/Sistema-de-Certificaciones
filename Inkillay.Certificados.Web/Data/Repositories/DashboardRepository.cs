using Dapper;
using SIGEC.Certificados.Web.Models.ViewModels;
using System.Data;

namespace SIGEC.Certificados.Web.Data.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public DashboardRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<AdminDashboardViewModel?> ObtenerEstadisticasDashboardAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        using var multi = await connection.QueryMultipleAsync(
            "USP_Dashboard_ObtenerEstadisticas",
            commandType: CommandType.StoredProcedure);

        var stats = await multi.ReadFirstOrDefaultAsync<AdminDashboardViewModel>();
        if (stats == null) stats = new AdminDashboardViewModel();

        var grafico = await multi.ReadAsync<RecaudacionMes>();
        stats.GraficoRecaudacion = grafico?.ToList() ?? new List<RecaudacionMes>();

        return stats;
    }

    public Task<IEnumerable<ActividadReciente>> ListarActividadRecienteAsync()
    {
        // Sin tabla Auditoria: retorna vacío sin lanzar excepción
        return Task.FromResult(Enumerable.Empty<ActividadReciente>());
    }

    public async Task<DocenteDashboardViewModel?> ObtenerStatsDocenteAsync(int idDocente)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<DocenteDashboardViewModel>(
            "USP_Dashboard_DocenteStats",
            new { IdDocente = idDocente },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<AlumnoDashboardViewModel?> ObtenerStatsAlumnoAsync(int idUsuario)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<AlumnoDashboardViewModel>(
            "USP_Dashboard_AlumnoStats",
            new { IdUsuario = idUsuario },
            commandType: CommandType.StoredProcedure);
    }
}
