namespace Inkillay.Certificados.Web.Services;

using Inkillay.Certificados.Web.Models.ViewModels;

public interface ICertificadoService
{
    // Recibe solo el nombre del archivo (no una ruta), para evitar LFI.
    byte[] GenerarImagenCertificado(string nombreImagen, string nombreAlumno, int ejeX, int ejeY, int fontSize, string fontColor);

    // Renderiza múltiples capas sobre la plantilla. Las capas con EsPrincipal=1 usan nombreAlumno.
    byte[] GenerarImagenCertificadoMulticapa(string nombreImagen, string nombreAlumno, List<PlantillaDetalleDTO> capas);

    // Convierte imagen JPEG a PDF
    byte[] GenerarPdfDesdeImagen(byte[] imagenBytes);
}
