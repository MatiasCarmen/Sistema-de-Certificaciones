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
        string procedure = "USP_Seg_Modulo_Listar";

        return await connection.QueryAsync<Seg_Modulo>(
            procedure,
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<Usuarios?> ValidarUsuarioAsync(string correo)
    {
        using var connection = _connectionFactory.CreateConnection();
        string procedure = "USP_Usuarios_ValidarAcceso";
        var parametros = new { Correo = correo };

        return await connection.QueryFirstOrDefaultAsync<Usuarios>(
            procedure,
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

    public async Task<int> InsertarPlantillaAsync(Plantilla plantilla)
    {
        using var connection = _connectionFactory.CreateConnection();
        var procedure = "USP_Plantillas_Insertar";
        var parametros = new
        {
            Nombre = plantilla.Nombre,
            RutaImagen = plantilla.RutaImagen
        };

        return await connection.ExecuteAsync(procedure, parametros, commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<Plantilla>> ListarPlantillasAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Plantilla>(
            "USP_Plantillas_ListarTodos",
            commandType: CommandType.StoredProcedure
        );
    }
}
