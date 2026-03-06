using Dapper;
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
        // 1. Abrimos la conexion usando nuestra factory
        using var connection = _connectionFactory.CreateConnection();

        // 2. Definimos el nombre del procedimiento almacenado
        string procedure = "USP_Seg_Modulo_Listar";

        // 3. Ejecutamos con Dapper indicando que es un StoredProcedure
        return await connection.QueryAsync<Seg_Modulo>(
            procedure,
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<Usuarios?> ValidarUsuarioAsync(string correo)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Nombre del procedimiento que creamos en SQL
        string procedure = "USP_Usuarios_ValidarAcceso";

        // Pasamos el parametro que espera el procedimiento
        var parametros = new { Correo = correo };

        return await connection.QueryFirstOrDefaultAsync<Usuarios>(
            procedure,
            parametros,
            commandType: CommandType.StoredProcedure
        );
    }
}
