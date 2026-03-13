using Dapper;
using Inkillay.Certificados.Web.Models;
using Inkillay.Certificados.Web.Models.Entities;
using Inkillay.Certificados.Web.Models.ViewModels;
using System.Data;
using System.Text.Json;

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

        return await connection.QueryFirstOrDefaultAsync<Usuarios>(
            "USP_Usuarios_ObtenerPorCorreo",
            parametros,
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

    public async Task<IEnumerable<Usuarios>> ListarUsuariosAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Usuarios>(
            "USP_Usuarios_ListarTodos",
            commandType: CommandType.StoredProcedure
        );
    }

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

    public async Task<bool> RegistrarUsuarioAsync(Usuarios usuario)
    {
        using var connection = _connectionFactory.CreateConnection();
        var resultado = await connection.ExecuteAsync(
            "USP_Usuarios_Registrar",
            new
            {
                usuario.Nombre,
                usuario.Correo,
                usuario.Clave,
                usuario.IdRol
            },
            commandType: CommandType.StoredProcedure
        );

        return resultado > 0;
    }

    public async Task<int> CambiarEstadoUsuarioAsync(int id, bool estado)
    {
        using var connection = _connectionFactory.CreateConnection();
        // ⚠️ MIGRACIÓN: Convertir bool a char ('A' o 'I') para el nuevo estándar
        char estadoChar = estado ? 'A' : 'I';
        return await connection.ExecuteAsync(
            "USP_Usuarios_CambiarEstado",
            new { IdUsuario = id, Estado = estadoChar },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<AdminDashboardViewModel?> ObtenerEstadisticasDashboardAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<AdminDashboardViewModel>(
            "USP_Dashboard_ObtenerEstadisticas",
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
            new { Nombre = plantilla.Nombre, RutaImagen = plantilla.RutaImagen },
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
            new { IdPlantilla = id, EjeX = x, EjeY = y, FontSize = fontSize, FontColor = fontColor },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<IEnumerable<PlantillaDetalleDTO>> ListarDetallesPlantillaAsync(int idPlantilla)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<PlantillaDetalleDTO>(
            "USP_PlantillaDetalle_Listar",
            new { IdPlantilla = idPlantilla },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<bool> GuardarDisenoCompletoAsync(int idPlantilla, List<PlantillaDetalleDTO> detalles)
    {
        using var connection = _connectionFactory.CreateConnection();
        // Serialize with PascalCase to match SQL Server OPENJSON column names
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = null };
        var json = JsonSerializer.Serialize(detalles, jsonOptions);

        await connection.ExecuteAsync(
            "USP_Plantillas_GuardarDisenoCompleto",
            new { IdPlantilla = idPlantilla, JsonDetalles = json },
            commandType: CommandType.StoredProcedure
        );

        // If no exception was thrown, the SP committed successfully
        return true;
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
}
