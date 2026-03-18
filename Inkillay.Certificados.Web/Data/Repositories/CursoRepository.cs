using Dapper;
using SIGEC.Certificados.Web.Models.Entities;
using SIGEC.Certificados.Web.Models.ViewModels;
using System.Data;

namespace SIGEC.Certificados.Web.Data.Repositories;

public class CursoRepository : ICursoRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public CursoRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Curso>> ListarTodosAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Curso>(
            "USP_Cursos_ListarTodos",
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<IEnumerable<Curso>> ListarCursosActivosAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Curso>(
            "USP_Cursos_ListarActivos",
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<Curso?> ObtenerPorIdAsync(int idCurso)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Curso>(
            "USP_Cursos_ObtenerPorId",
            new { IdCurso = idCurso },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<IEnumerable<ReporteCursoViewModel>> ObtenerReporteDocenteAsync(int idProfesor)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<ReporteCursoViewModel>(
            "USP_Profesor_ReporteGeneral",
            new { IdProfesor = idProfesor },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<int> RegistrarCursoAsync(Curso curso)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(
            "USP_Cursos_Registrar",
            new
            {
                curso.Nombre,
                curso.Sumilla,
                curso.Costo,
                curso.FechaInicio,
                curso.FechaFin,
                curso.IdProfesor,
                curso.Imagen,
                curso.UsuarioRegistro
            },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<bool> ActualizarCursoAsync(Curso curso)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.ExecuteAsync(
            "USP_Cursos_Actualizar",
            new
            {
                curso.IdCurso,
                curso.Nombre,
                curso.Sumilla,
                curso.Costo,
                curso.FechaInicio,
                curso.FechaFin,
                curso.IdProfesor,
                curso.Imagen,
                UsuarioModifica = curso.UsuarioModifica
            },
            commandType: CommandType.StoredProcedure
        );
        return result > 0;
    }
}
