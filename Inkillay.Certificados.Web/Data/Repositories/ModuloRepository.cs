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
        return await connection.QueryAsync<Modulo>(
            "USP_Modulos_ListarTodos",
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<Modulo?> ObtenerPorIdAsync(int idModulo)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Modulo>(
            "USP_Modulos_ObtenerPorId",
            new { IdModulo = idModulo },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<IEnumerable<Modulo>> ListarPorCursoAsync(int idCurso)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Modulo>(
            "USP_Modulos_ListarPorCurso",
            new { IdCurso = idCurso },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<int> RegistrarModuloAsync(Modulo modulo)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameter = new DynamicParameters();
        parameter.Add("@IdCurso", modulo.IdCurso);
        parameter.Add("@IdDocente", modulo.IdDocente);
        parameter.Add("@EdicionCurso", modulo.EdicionCurso);
        parameter.Add("@FechaInicioMatricula", modulo.FechaInicioMatricula);
        parameter.Add("@FechaFinMatricula", modulo.FechaFinMatricula);
        parameter.Add("@FechaInicioClases", modulo.FechaInicioClases);
        parameter.Add("@CostoCurso", modulo.CostoCurso);
        parameter.Add("@CapacidadAlumno", modulo.CapacidadAlumno);
        parameter.Add("@Modalidad", modulo.Modalidad);
        parameter.Add("@UsuarioRegistro", modulo.UsuarioRegistro);
        parameter.Add("@IdModulo", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync("USP_Modulos_Registrar", parameter, commandType: CommandType.StoredProcedure);
        return parameter.Get<int>("@IdModulo");
    }

    public async Task<bool> ActualizarModuloAsync(Modulo modulo)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameter = new DynamicParameters();
        parameter.Add("@IdModulo", modulo.IdModulo);
        parameter.Add("@IdCurso", modulo.IdCurso);
        parameter.Add("@IdDocente", modulo.IdDocente);
        parameter.Add("@EdicionCurso", modulo.EdicionCurso);
        parameter.Add("@FechaInicioMatricula", modulo.FechaInicioMatricula);
        parameter.Add("@FechaFinMatricula", modulo.FechaFinMatricula);
        parameter.Add("@FechaInicioClases", modulo.FechaInicioClases);
        parameter.Add("@CostoCurso", modulo.CostoCurso);
        parameter.Add("@CapacidadAlumno", modulo.CapacidadAlumno);
        parameter.Add("@Modalidad", modulo.Modalidad);
        parameter.Add("@Estado", modulo.Estado);
        parameter.Add("@UsuarioModifica", modulo.UsuarioModifica);

        var result = await connection.ExecuteAsync("USP_Modulos_Actualizar", parameter, commandType: CommandType.StoredProcedure);
        return result > 0;
    }

    public async Task<IEnumerable<ModuloAlumno>> ListarAlumnosPorModuloAsync(int idModulo)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<ModuloAlumno>(
            "USP_ModuloAlumnos_ListarPorModulo",
            new { IdModulo = idModulo },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<IEnumerable<ModuloAlumno>> ListarModulosPorAlumnoAsync(int idUsuario)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<ModuloAlumno>(
            "USP_Alumnos_ListarMisModulos",
            new { IdUsuario = idUsuario },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<bool> RegistrarModuloAlumnoAsync(ModuloAlumno moduloAlumno)
    {
        using var connection = _connectionFactory.CreateConnection();
        var filas = await connection.ExecuteAsync(
            "USP_ModuloAlumnos_Registrar",
            new {
                IdModulo = moduloAlumno.IdModulo,
                IdUsuario = moduloAlumno.IdUsuario,
                UsuarioRegistro = moduloAlumno.UsuarioRegistro
            },
            commandType: CommandType.StoredProcedure
        );
        return filas > 0;
    }

    public async Task<bool> ActualizarNotaFinalAsync(int idModuloAlumno, decimal notaFinal, string usuarioModifica)
    {
        using var connection = _connectionFactory.CreateConnection();
        var filas = await connection.ExecuteAsync(
            "USP_ModuloAlumnos_ActualizarNota",
            new {
                IdModuloAlumno = idModuloAlumno,
                NotaFinal = notaFinal,
                UsuarioModifica = usuarioModifica
            },
            commandType: CommandType.StoredProcedure
        );
        return filas > 0;
    }

    // LEGACY IMPLEMENTATIONS
    public async Task<IEnumerable<Modulo>> ListarAlumnosPorCursoAsync(int idCurso)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Modulo>(
            "USP_Modulos_ListarAlumnosPorCurso",
            new { IdCurso = idCurso },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<IEnumerable<Modulo>> ListarCursosPorAlumnoAsync(int idUsuario)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Modulo>(
            "USP_Alumnos_ListarMisCursos",
            new { IdAlumno = idUsuario },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<bool> RegistrarModuloAsync(int idAlumno, int idCurso, char modalidad, string usuarioRegistro = "Sistema")
    {
        using var connection = _connectionFactory.CreateConnection();
        var filas = await connection.ExecuteAsync(
            "USP_Modulos_Registrar_Legacy", // En un futuro el DBA debería adaptar un sp viejo. Pero dejo el de siempre
            new { IdAlumno = idAlumno, IdCurso = idCurso, Modalidad = modalidad, UsuarioRegistro = usuarioRegistro },
            commandType: CommandType.StoredProcedure
        );
        return filas > 0;
    }
}
