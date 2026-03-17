using Dapper;
using Inkillay.Certificados.Web.Models.ViewModels;
using System.Data;

namespace Inkillay.Certificados.Web.Data.Repositories;

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
        return await connection.QueryFirstOrDefaultAsync<AdminDashboardViewModel>(
            "USP_Dashboard_ObtenerEstadisticas",
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<IEnumerable<RecentActivityViewModel>> ListarActividadRecienteAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<RecentActivityViewModel>(
            "USP_Diplomas_ListarRecientes",
            commandType: CommandType.StoredProcedure
        );
    }
}
