using SkiaSharp;

namespace Inkillay.Certificados.Web.Services;

public class CertificadoService : ICertificadoService
{
    public byte[] GenerarImagenCertificado(string rutaPlantilla, string nombreAlumno, int ejeX, int ejeY, int fontSize, string fontColor)
    {
        using var input = File.OpenRead(rutaPlantilla);
        using var bitmap = SKBitmap.Decode(input);
        using var canvas = new SKCanvas(bitmap);

        // Configuracion del texto
        var paint = new SKPaint
        {
            Color = SKColor.Parse(string.IsNullOrWhiteSpace(fontColor) ? "#000000" : fontColor),
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            TextSize = fontSize,
            Typeface = SKTypeface.FromFamilyName(
                "Arial",
                SKFontStyleWeight.Bold,
                SKFontStyleWidth.Normal,
                SKFontStyleSlant.Upright)
        };

        // Dibujar el nombre en las coordenadas guardadas
        canvas.DrawText(nombreAlumno, ejeX, ejeY, paint);

        // Convertir el resultado a bytes
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
        return data.ToArray();
    }
}
