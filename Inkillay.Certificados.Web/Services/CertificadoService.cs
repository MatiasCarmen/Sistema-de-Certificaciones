using PdfSharp.Drawing;
using PdfSharp.Pdf;
using SkiaSharp;

namespace Inkillay.Certificados.Web.Services;

public class CertificadoService : ICertificadoService
{
    private readonly IWebHostEnvironment _hostEnvironment;

    public CertificadoService(IWebHostEnvironment hostEnvironment)
    {
        _hostEnvironment = hostEnvironment;
    }

    public byte[] GenerarImagenCertificado(string nombreImagen, string nombreAlumno, int ejeX, int ejeY, int fontSize, string fontColor)
    {
       
        string nombreLimpio = Path.GetFileName(nombreImagen);

        
        if (nombreLimpio != nombreImagen)
        {
            throw new UnauthorizedAccessException("Intento de acceso a ruta no permitida");
        }

        
        string carpetaUploads = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
        string rutaFinal = Path.Combine(carpetaUploads, nombreLimpio);

       
        string fullPathFinal = Path.GetFullPath(rutaFinal);
        string fullPathUploads = Path.GetFullPath(carpetaUploads);

        if (!fullPathFinal.StartsWith(fullPathUploads, StringComparison.OrdinalIgnoreCase))
        {
            
            throw new UnauthorizedAccessException("Intento de acceso a ruta no permitida");
        }

        
        if (!File.Exists(fullPathFinal))
        {
            throw new FileNotFoundException("La plantilla no existe en el servidor");
        }

        
        using var input = File.OpenRead(fullPathFinal);
        using var bitmap = SKBitmap.Decode(input);
        if (bitmap == null)
        {
            throw new InvalidOperationException("No se pudo decodificar la imagen de la plantilla");
        }

        using var canvas = new SKCanvas(bitmap);

        SKColor color;
        try
        {
            color = SKColor.Parse(string.IsNullOrWhiteSpace(fontColor) ? "#000000" : fontColor);
        }
        catch
        {
            color = SKColors.Black;
        }

        var paint = new SKPaint
        {
            Color = color,
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

    public byte[] GenerarPdfDesdeImagen(byte[] imagenBytes)
    {
        using (var msInput = new MemoryStream(imagenBytes))
        using (var msOutput = new MemoryStream())
        {
            // 1. Crear documento PDF
            PdfDocument document = new PdfDocument();
            PdfPage page = document.AddPage();

            // 2. Cargar la imagen generada por SkiaSharp
            XImage image = XImage.FromStream(msInput);

            // 3. Ajustar tamaño de página al tamaño de la imagen (manteniendo calidad)
            page.Width = image.PointWidth;
            page.Height = image.PointHeight;

            // 4. Dibujar la imagen en el PDF
            XGraphics gfx = XGraphics.FromPdfPage(page);
            gfx.DrawImage(image, 0, 0);

            // 5. Guardar y retornar
            document.Save(msOutput, false);
            return msOutput.ToArray();
        }
    }
}

