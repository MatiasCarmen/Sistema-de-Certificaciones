using Dapper;
using Inkillay.Certificados.Web.Models.Entities;
using Inkillay.Certificados.Web.Models.ViewModels;
using System.Data;
using System.Text.Json;

namespace Inkillay.Certificados.Web.Data.Repositories;

public class PlantillaRepository : IPlantillaRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public PlantillaRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
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
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = null };
        var json = JsonSerializer.Serialize(detalles, jsonOptions);

        await connection.ExecuteAsync(
            "USP_Plantillas_GuardarDisenoCompleto",
            new { IdPlantilla = idPlantilla, JsonDetalles = json },
            commandType: CommandType.StoredProcedure
        );

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
