using Dapper;
using Inkillay.Certificados.Web.Models.Entities;
using System.Data;

namespace Inkillay.Certificados.Web.Data.Repositories;

public class ModuloRepository(DbConnectionFactory _connectionFactory) : IModuloRepository
{

    public async Task<IEnumerable<Modulo>> ListarTodosAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Modulo>("USP_Modulos_ListarTodos", commandType: CommandType.StoredProcedure);
    }

    public async Task<Modulo?> ObtenerPorIdAsync(int idModulo)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = "SELECT * FROM Modulos WHERE IdModulo = @idModulo";
        return await connection.QueryFirstOrDefaultAsync<Modulo>(sql, new { idModulo });
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
        var sql = @"
            UPDATE Modulos SET 
                IdCurso = @IdCurso,
                IdDocente = @IdDocente,
                EdicionCurso = @EdicionCurso,
                CostoCurso = @CostoCurso,
                CapacidadAlumno = @CapacidadAlumno,
                FechaInicioMatricula = @FechaInicioMatricula,
                FechaFinMatricula = @FechaFinMatricula,
                FechaInicioClases = @FechaInicioClases,
                Modalidad = @Modalidad,
                EstadoMatricula = @EstadoMatricula,
                UsuarioModifica = @UsuarioModifica,
                FechaModificacion = GETDATE()
            WHERE IdModulo = @IdModulo";

        var filas = await connection.ExecuteAsync(sql, modulo);
        return filas > 0;
    }

    public async Task<bool> EliminarModuloAsync(int idModulo)
    {
        using var connection = _connectionFactory.CreateConnection();
        var filas = await connection.ExecuteAsync(
            "DELETE FROM Modulos WHERE IdModulo = @idModulo",
            new { idModulo });
        return filas > 0;
    }
}
