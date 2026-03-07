using SkiaSharp;

namespace Inkillay.Certificados.Web.Services;

public class CertificadoService : ICertificadoService
{
    public byte[] GenerarImagenCertificado(string rutaPlantilla, string nombreAlumno, int ejeX, int ejeY, int fontSize, string fontColor)
    {
        using var input = File.OpenRead(rutaPlantilla);
        using var bitmap = SKBitmap.Decode(input);
        using var canvas = new SKCanvas(bitmap);

        var paint = new SKPaint
        {
            Color = SKColor.Parse(string.IsNullOrWhiteSpace(fontColor) ? "#000000" : fontColor),
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            TextAlign = SKTextAlign.Left,
            TextSize = fontSize,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
        };

        canvas.DrawText(nombreAlumno, ejeX, ejeY, paint);

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
        return data.ToArray();
    }
}
