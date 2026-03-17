using Dapper;
using Inkillay.Certificados.Web.Models.Entities;
using System.Data;

namespace Inkillay.Certificados.Web.Data.Repositories;

public class ModuloRepository : IModuloRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public ModuloRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Modulo>> ListarTodosAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Modulo>("USP_Modulos_ListarTodos", commandType: CommandType.StoredProcedure);
    }

    public async Task<Modulo?> ObtenerPorIdAsync(int idModulo)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Modulo>("USP_Modulos_ObtenerPorId", new { idModulo }, commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<Modulo>> ListarPorCursoAsync(int idCurso)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Modulo>("USP_Modulos_ListarPorCurso", new { idCurso }, commandType: CommandType.StoredProcedure);
    }

    public async Task<int> RegistrarModuloAsync(Modulo modulo)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(
            "USP_Modulos_Insertar",
            new
            {
                modulo.IdCurso,
                modulo.IdDocente,
                modulo.EdicionCurso,
                modulo.CostoCurso,
                modulo.CapacidadAlumno,
                modulo.FechaInicioMatricula,
                modulo.FechaFinMatricula,
                modulo.FechaInicioClases,
                modulo.Modalidad,
                modulo.EstadoMatricula,
                modulo.UsuarioRegistro
            },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<bool> ActualizarModuloAsync(Modulo modulo)
    {
        using var connection = _connectionFactory.CreateConnection();
        var filas = await connection.ExecuteAsync(
            "USP_Modulos_Actualizar",
            new
            {
                modulo.IdModulo,
                modulo.IdCurso,
                modulo.IdDocente,
                modulo.EdicionCurso,
                modulo.CostoCurso,
                modulo.CapacidadAlumno,
                modulo.FechaInicioMatricula,
                modulo.FechaFinMatricula,
                modulo.FechaInicioClases,
                modulo.Modalidad,
                modulo.EstadoMatricula,
                modulo.UsuarioModifica
            },
            commandType: CommandType.StoredProcedure
        );
        return filas > 0;
    }
}
