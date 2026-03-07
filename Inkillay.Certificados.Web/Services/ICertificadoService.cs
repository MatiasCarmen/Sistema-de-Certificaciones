namespace Inkillay.Certificados.Web.Services;

public interface ICertificadoService
{
    byte[] GenerarImagenCertificado(string rutaPlantilla, string nombreAlumno, int ejeX, int ejeY, int fontSize, string fontColor);
}
