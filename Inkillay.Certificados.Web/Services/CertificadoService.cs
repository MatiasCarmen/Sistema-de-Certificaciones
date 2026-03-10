using Inkillay.Certificados.Web.Models.ViewModels;
using SkiaSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.IO;

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
        if (string.IsNullOrWhiteSpace(nombreImagen))
        {
            throw new ArgumentException("Nombre de imagen invalido", nameof(nombreImagen));
        }

        // Contencion: el servicio solo resuelve dentro de /wwwroot/uploads.
        var carpetaUploads = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
        var uploadsFull = EnsureTrailingSeparator(Path.GetFullPath(carpetaUploads));

        // Quita cualquier intento de path traversal (..\ etc).
        var safeFileName = Path.GetFileName(nombreImagen);
        if (string.IsNullOrWhiteSpace(safeFileName))
        {
            throw new ArgumentException("Nombre de imagen invalido", nameof(nombreImagen));
        }

        var rutaCandidata = Path.Combine(carpetaUploads, safeFileName);
        var rutaFull = Path.GetFullPath(rutaCandidata);

        if (!rutaFull.StartsWith(uploadsFull, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Intento de acceso a ruta no permitida");
        }

        if (!File.Exists(rutaFull))
        {
            throw new FileNotFoundException("No se encontro la plantilla en uploads", rutaFull);
        }

        using var input = File.OpenRead(rutaFull);
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

    public byte[] GenerarImagenCertificadoMulticapa(string nombreImagen, string nombreAlumno, List<PlantillaDetalleDTO> capas)
    {
        if (string.IsNullOrWhiteSpace(nombreImagen))
            throw new ArgumentException("Nombre de imagen invalido", nameof(nombreImagen));

        var carpetaUploads = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
        var uploadsFull = EnsureTrailingSeparator(Path.GetFullPath(carpetaUploads));

        var safeFileName = Path.GetFileName(nombreImagen);
        if (string.IsNullOrWhiteSpace(safeFileName))
            throw new ArgumentException("Nombre de imagen invalido", nameof(nombreImagen));

        var rutaCandidata = Path.Combine(carpetaUploads, safeFileName);
        var rutaFull = Path.GetFullPath(rutaCandidata);

        if (!rutaFull.StartsWith(uploadsFull, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("Intento de acceso a ruta no permitida");

        if (!File.Exists(rutaFull))
            throw new FileNotFoundException("No se encontro la plantilla en uploads", rutaFull);

        using var input = File.OpenRead(rutaFull);
        using var bitmap = SKBitmap.Decode(input);
        if (bitmap == null)
            throw new InvalidOperationException("No se pudo decodificar la imagen de la plantilla");

        using var canvas = new SKCanvas(bitmap);

        foreach (var capa in capas.OrderBy(c => c.Orden))
        {
            string texto = capa.EsPrincipal == 1 ? nombreAlumno : capa.Texto;
            if (string.IsNullOrEmpty(texto)) continue;

            SKColor color;
            try { color = SKColor.Parse(string.IsNullOrWhiteSpace(capa.FontColor) ? "#000000" : capa.FontColor); }
            catch { color = SKColors.Black; }

            var paint = new SKPaint
            {
                Color = color,
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                TextAlign = SKTextAlign.Left,
                TextSize = capa.FontSize > 0 ? capa.FontSize : 40,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
            };

            canvas.DrawText(texto, capa.X, capa.Y, paint);
        }

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
        return data.ToArray();
    }

    public byte[] GenerarPdfDesdeImagen(byte[] imagenBytes)
    {
        if (imagenBytes == null || imagenBytes.Length == 0)
        {
            throw new ArgumentException("Los bytes de la imagen no pueden estar vacíos", nameof(imagenBytes));
        }

        using var memoryStream = new MemoryStream(imagenBytes);
        using var bitmap = SKBitmap.Decode(memoryStream);
        if (bitmap == null)
        {
            throw new InvalidOperationException("No se pudo decodificar la imagen proporcionada");
        }

        using (var document = new PdfDocument())
        {
            var page = document.AddPage();

            // Configurar el tamaño de la página según las dimensiones de la imagen
            page.Width = XUnit.FromPoint(bitmap.Width);
            page.Height = XUnit.FromPoint(bitmap.Height);

            using (var gfx = XGraphics.FromPdfPage(page))
            {
                using var tempStream = new MemoryStream(imagenBytes);
                var xImage = XImage.FromStream(tempStream);
                gfx.DrawImage(xImage, 0, 0, page.Width.Point, page.Height.Point);
            }

            using var pdfStream = new MemoryStream();
            document.Save(pdfStream, false);
            return pdfStream.ToArray();
        }
    }

    private static string EnsureTrailingSeparator(string path)
    {
        if (string.IsNullOrEmpty(path)) return path;
        var sep = Path.DirectorySeparatorChar;
        return path.EndsWith(sep) ? path : path + sep;
    }
}
