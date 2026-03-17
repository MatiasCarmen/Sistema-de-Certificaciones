using Dapper;
using Inkillay.Certificados.Web.Models;
using Inkillay.Certificados.Web.Models.Entities;
using Inkillay.Certificados.Web.Models.ViewModels;
using System.Data;
using System.Text.Json;
using Inkillay.Certificados.Web.Services;

namespace Inkillay.Certificados.Web.Data.Repositories;

public class SeguridadRepository : ISeguridadRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public SeguridadRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    // --- MÉTODOS DE MÓDULOS ---

    public async Task<IEnumerable<Seg_Modulo>> ListarModulosAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Seg_Modulo>(
            "USP_Modulos_ListarTodos",
            commandType: CommandType.StoredProcedure
        );
    }

    // --- MÉTODOS DE USUARIOS ---

    // Agregamos este alias para que el ModulosController funcione sin cambios
    public async Task<IEnumerable<Usuarios>> ListarTodosAsync()
    {
        return await ListarUsuariosAsync();
    }

    public async Task<IEnumerable<Usuarios>> ListarUsuariosAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Usuarios>(
            "USP_Usuarios_ListarTodos",
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

    public async Task<Usuarios?> ObtenerUsuarioPorCorreoAsync(string correo)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Usuarios>(
            "USP_Usuarios_ObtenerPorCorreo",
            new { Correo = correo },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<Usuarios?> ObtenerUsuarioPorIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Usuarios>(
            "USP_Usuarios_ObtenerPorId",
            new { IdUsuario = id },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<bool> RegistrarUsuarioAsync(Usuarios usuario)
    {
        using var connection = _connectionFactory.CreateConnection();
        var id = await connection.ExecuteScalarAsync(
            "USP_Usuarios_Registrar",
            new
            {
                Nombre = usuario.Nombre,
                Correo = usuario.Correo,
                Clave = usuario.Clave,
                IdRol = usuario.IdRol,
                UsuarioRegistro = usuario.UsuarioRegistro
            },
            commandType: CommandType.StoredProcedure
        );

        return id != null && Convert.ToInt32(id) > 0;
    }

    // --- MÉTODOS DE ALUMNOS (FICHA COMPLETA) ---

    public async Task<EditarAlumnoViewModel?> ObtenerAlumnoPorIdAsync(int idUsuario)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<EditarAlumnoViewModel>(
            "USP_Alumno_ObtenerPorId",
            new { IdUsuario = idUsuario },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<bool> RegistrarAlumnoCompletoAsync(CrearUsuarioViewModel modelo, string usuarioRegistro)
    {
        using var connection = _connectionFactory.CreateConnection();
        var id = await connection.ExecuteScalarAsync(
            "USP_Alumno_RegistrarCompleto",
            new
            {
                Nombres = modelo.Nombre.Trim(),
                ApellidoPaterno = modelo.ApellidoPaterno?.Trim() ?? string.Empty,
                ApellidoMaterno = modelo.ApellidoMaterno?.Trim() ?? string.Empty,
                FechaNacimiento = modelo.FechaNacimiento,
                Ciudad = modelo.Ciudad?.Trim() ?? string.Empty,
                Telefono = modelo.Telefono?.Trim() ?? string.Empty,
                TipoDocumento = modelo.TipoDocumento?.Trim() ?? string.Empty,
                NumeroDocumento = modelo.NumeroDocumento?.Trim() ?? string.Empty,
                Correo = modelo.Correo.Trim().ToLower(),
                Clave = modelo.Clave,
                UsuarioRegistro = usuarioRegistro
            },
            commandType: CommandType.StoredProcedure
        );

        return id != null && Convert.ToInt32(id) > 0;
    }

    public async Task<bool> ActualizarAlumnoCompletoAsync(EditarAlumnoViewModel modelo, string usuarioModifica)
    {
        using var connection = _connectionFactory.CreateConnection();
        var id = await connection.ExecuteScalarAsync(
            "USP_Alumno_ActualizarCompleto",
            new
            {
                IdUsuario = modelo.IdUsuario,
                Nombres = modelo.Nombres.Trim(),
                ApellidoPaterno = modelo.ApellidoPaterno?.Trim() ?? string.Empty,
                ApellidoMaterno = modelo.ApellidoMaterno?.Trim() ?? string.Empty,
                FechaNacimiento = modelo.FechaNacimiento,
                Ciudad = modelo.Ciudad?.Trim() ?? string.Empty,
                Telefono = modelo.Telefono?.Trim() ?? string.Empty,
                TipoDocumento = modelo.TipoDocumento?.Trim() ?? string.Empty,
                NumeroDocumento = modelo.NumeroDocumento?.Trim() ?? string.Empty,
                Correo = modelo.Correo.Trim().ToLower(),
                ClaveHash = string.IsNullOrWhiteSpace(modelo.ClaveNueva) ? null : HashHelper.HashPassword(modelo.ClaveNueva),
                UsuarioModifica = usuarioModifica
            },
            commandType: CommandType.StoredProcedure
        );

        return id != null && Convert.ToInt32(id) > 0;
    }

    // --- OTROS MÉTODOS ---

    public async Task<bool> CorreoExisteAsync(string correo)
    {
        using var connection = _connectionFactory.CreateConnection();
        var resultado = await connection.QueryFirstOrDefaultAsync<int>(
            "USP_Usuarios_CorreoExiste",
            new { Correo = correo },
            commandType: CommandType.StoredProcedure
        );
        return resultado > 0;
    }

    public async Task<int> CambiarEstadoUsuarioAsync(int id, bool estado)
    {
        using var connection = _connectionFactory.CreateConnection();
        char estadoChar = estado ? 'A' : 'I';
        return await connection.ExecuteAsync(
            "USP_Usuarios_CambiarEstado",
            new { IdUsuario = id, Estado = estadoChar },
            commandType: CommandType.StoredProcedure
        );
    }
}
