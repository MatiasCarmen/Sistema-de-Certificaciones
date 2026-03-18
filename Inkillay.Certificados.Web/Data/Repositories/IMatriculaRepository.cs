using Dapper;
using SIGEC.Certificados.Web.Models.Entities;
using System.Data;

namespace SIGEC.Certificados.Web.Data.Repositories;

public interface IMatriculaRepository
{
    Task<IEnumerable<Matricula>> ListarAlumnosPorModuloAsync(int idModulo);
    Task<IEnumerable<Matricula>> ListarModulosPorAlumnoAsync(int idUsuario);
    Task<Matricula?> ObtenerPorIdAsync(int idMatricula);
    Task<bool> RegistrarMatriculaAsync(int idModulo, int idUsuario, string usuarioRegistro);
    Task<bool> ActualizarNotaFinalAsync(int idMatricula, decimal notaFinal, string usuarioModifica);
}

public class MatriculaRepository(DbConnectionFactory _connectionFactory) : IMatriculaRepository
{
    public async Task<IEnumerable<Matricula>> ListarAlumnosPorModuloAsync(int idModulo)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Matricula>(
            "USP_Matriculas_ListarAlumnosPorCurso", 
            new { IdModulo = idModulo },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<IEnumerable<Matricula>> ListarModulosPorAlumnoAsync(int idUsuario)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Matricula>(
            "USP_Alumnos_ListarMisModulos",
            new { IdUsuario = idUsuario },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<Matricula?> ObtenerPorIdAsync(int idMatricula)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Matricula>(
            "USP_Matriculas_ObtenerPorId",
            new { IdMatricula = idMatricula },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<bool> RegistrarMatriculaAsync(int idModulo, int idUsuario, string usuarioRegistro)
    {
        using var connection = _connectionFactory.CreateConnection();
        var filas = await connection.ExecuteAsync(
            "USP_Matriculas_Registrar", 
            new { IdModulo = idModulo, IdUsuario = idUsuario, UsuarioRegistro = usuarioRegistro },
            commandType: CommandType.StoredProcedure
        );
        return filas > 0;
    }

    public async Task<bool> ActualizarNotaFinalAsync(int idMatricula, decimal notaFinal, string usuarioModifica)
    {
        using var connection = _connectionFactory.CreateConnection();
        var filas = await connection.ExecuteAsync(
            "USP_ModuloAlumnos_ActualizarNota", 
            new { IdModuloAlumno = idMatricula, NotaFinal = notaFinal, UsuarioModifica = usuarioModifica },
            commandType: CommandType.StoredProcedure
        );
        return filas > 0;
    }
}
