namespace Inkillay.Certificados.Web.Services;

public interface ICertificadoService
{
    // Recibe solo el nombre del archivo (no una ruta), para evitar LFI.
    byte[] GenerarImagenCertificado(string nombreImagen, string nombreAlumno, int ejeX, int ejeY, int fontSize, string fontColor);

    // Convierte imagen JPEG a PDF
    byte[] GenerarPdfDesdeImagen(byte[] imagenBytes);
}
