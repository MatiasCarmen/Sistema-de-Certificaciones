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

    public async Task<Usuarios> ObtenerUsuarioPorCorreoAsync(string correo)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parametros = new { Correo = correo };

        // Invocamos el SP que ya creamos en Grape
        return await connection.QueryFirstOrDefaultAsync<Usuarios>(
            "USP_Usuarios_ObtenerPorCorreo",
            parametros,
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
        return await connection.ExecuteAsync(
            "USP_Plantillas_CambiarEstado",
            new { IdPlantilla = id },
            commandType: CommandType.StoredProcedure
        );
    }
}
