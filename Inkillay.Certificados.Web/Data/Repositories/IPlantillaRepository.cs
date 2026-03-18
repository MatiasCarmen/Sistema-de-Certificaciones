using SIGEC.Certificados.Web.Models.Entities;
using SIGEC.Certificados.Web.Models.ViewModels;

namespace SIGEC.Certificados.Web.Data.Repositories;

public interface IPlantillaRepository
{
    Task<IEnumerable<Plantilla>> ListarPlantillasAsync();
    Task<int> InsertarPlantillaAsync(Plantilla plantilla);
    Task<int> ActualizarCoordenadasAsync(int id, int x, int y);
    Task<int> ActualizarDisenoPlantillaAsync(int id, int x, int y, int fontSize, string fontColor);
    Task<IEnumerable<PlantillaDetalleDTO>> ListarDetallesPlantillaAsync(int idPlantilla);
    Task<bool> GuardarDisenoCompletoAsync(int idPlantilla, List<PlantillaDetalleDTO> detalles);
    Task<int> CambiarEstadoPlantillaAsync(int id);
    Task<int> EliminarPlantillaAsync(int id);
}
