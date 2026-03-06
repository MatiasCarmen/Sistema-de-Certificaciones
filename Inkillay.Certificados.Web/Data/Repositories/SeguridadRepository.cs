using Dapper;
using Inkillay.Certificados.Web.Models;
using Inkillay.Certificados.Web.Models.Entities;
using System.Data;

namespace Inkillay.Certificados.Web.Data.Repositories;

public class SeguridadRepository : ISeguridadRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public SeguridadRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Seg_Modulo>> ListarModulosAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Seg_Modulo>(
            "USP_Seg_Modulo_Listar",
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<Usuarios?> ValidarUsuarioAsync(string correo)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Usuarios>(
            "USP_Usuarios_ValidarAcceso",
            new { Correo = correo },
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

    public async Task<IEnumerable<Plantilla>> ListarPlantillasAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Plantilla>(
            "USP_Plantillas_ListarTodos",
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<int> InsertarPlantillaAsync(Plantilla plantilla)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(
            "USP_Plantillas_Insertar",
            new
            {
                Nombre = plantilla.Nombre,
                RutaImagen = plantilla.RutaImagen
            },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<int> ActualizarCoordenadasAsync(int id, int x, int y)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(
            "USP_Plantillas_ActualizarCoordenadas",
            new { IdPlantilla = id, EjeX = x, EjeY = y },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<int> CambiarEstadoPlantillaAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            UPDATE Plantillas
            SET Estado = CASE WHEN Estado = 1 THEN 0 ELSE 1 END
            WHERE IdPlantilla = @IdPlantilla;
            """;

        return await connection.ExecuteAsync(sql, new { IdPlantilla = id });
    }
}
