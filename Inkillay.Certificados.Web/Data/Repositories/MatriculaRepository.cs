using Dapper;
using Inkillay.Certificados.Web.Models.Entities;
using System.Data;

namespace Inkillay.Certificados.Web.Data.Repositories;

public class MatriculaRepository : IMatriculaRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public MatriculaRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Matricula>> ListarAlumnosPorCursoAsync(int idCurso)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Matricula>(
            "USP_Matriculas_ListarAlumnosPorCurso",
            new { IdCurso = idCurso },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<IEnumerable<Matricula>> ListarCursosPorAlumnoAsync(int idAlumno)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Matricula>(
            "USP_Alumnos_ListarMisCursos",
            new { IdAlumno = idAlumno },
            commandType: CommandType.StoredProcedure
        );
    }
}
