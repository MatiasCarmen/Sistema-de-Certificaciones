using Dapper;
using Inkillay.Certificados.Web.Models;
using Inkillay.Certificados.Web.Models.Entities;
using Inkillay.Certificados.Web.Models.ViewModels;
using Inkillay.Certificados.Web.Services;
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

    public async Task<int> ActualizarDisenoPlantillaAsync(int id, int x, int y, int fontSize, string fontColor)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(
            "USP_Plantillas_ActualizarDiseno",
            new
            {
                IdPlantilla = id,
                EjeX = x,
                EjeY = y,
                FontSize = fontSize,
                FontColor = fontColor
            },
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



    public async Task<IEnumerable<UsuarioViewModel>> ListarUsuariosAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<UsuarioViewModel>(
            "USP_Usuarios_Listar",
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<Usuarios?> ObtenerUsuarioPorIdAsync(int idUsuario)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Usuarios>(
            "USP_Usuarios_ObtenerPorId",
            new { IdUsuario = idUsuario },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<bool> RegistrarUsuarioAsync(Usuarios usuario)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Hashear la contraseña antes de guardar
        var claveHash = HashHelper.HashPassword(usuario.Clave);

        var filas = await connection.ExecuteAsync(
            "USP_Usuarios_Registrar",
            new
            {
                Nombre = usuario.Nombre,
                Correo = usuario.Correo,
                Clave = claveHash,
                IdRol = usuario.IdRol
            },
            commandType: CommandType.StoredProcedure
        );

        return filas > 0;
    }

    public async Task<bool> CorreoExisteAsync(string correo)
    {
        using var connection = _connectionFactory.CreateConnection();
        var resultado = await connection.QueryFirstOrDefaultAsync<int>(
            "USP_Usuarios_VerificarCorreo",
            new { Correo = correo },
            commandType: CommandType.StoredProcedure
        );

        return resultado > 0;
    }

    public async Task<int> CambiarEstadoUsuarioAsync(int idUsuario, bool estado)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(
            "USP_Usuarios_CambiarEstado",
            new { IdUsuario = idUsuario, Estado = estado },
            commandType: CommandType.StoredProcedure
        );
    }

    // ===== DASHBOARD ADMIN =====

    public async Task<AdminDashboardViewModel> ObtenerEstadisticasDashboardAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        try
        {
            using var reader = await connection.QueryMultipleAsync(
                "USP_Admin_ObtenerEstadisticas",
                commandType: CommandType.StoredProcedure
            );

            // Leer primer resultado set (Estadísticas generales)
            var estadisticas = await reader.ReadFirstOrDefaultAsync<EstadisticasGenerales>();

            // Leer segundo resultado set (Recaudación por mes)
            var recaudacionMes = (await reader.ReadAsync<RecaudacionMes>()).ToList();

            var dashboard = new AdminDashboardViewModel
            {
                TotalAlumnos = estadisticas?.TotalAlumnos ?? 0,
                CursosActivos = estadisticas?.CursosActivos ?? 0,
                RecaudacionTotal = estadisticas?.RecaudacionTotal ?? 0,
                CertificadosEmitidos = estadisticas?.CertificadosEmitidos ?? 0,
                GraficoRecaudacion = recaudacionMes
            };

            return dashboard;
        }
        catch (Exception ex)
        {
            // Log the exception if needed
            return new AdminDashboardViewModel();
        }
    }
}
