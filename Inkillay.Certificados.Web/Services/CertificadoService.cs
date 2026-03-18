using SIGEC.Certificados.Web.Models.ViewModels;
using SkiaSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.IO;

namespace SIGEC.Certificados.Web.Services;

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
            TextAlign = SKTextAlign.Center,
            TextSize = fontSize,
            Typeface = ResolveTypeface()
        };

        // CSS positions from top-left; SkiaSharp DrawText uses baseline.
        // MeasureText returns bounds where bounds.Top is negative (ascent above baseline).
        // Subtracting it converts top-left Y to baseline Y with pixel-perfect precision.
        SKRect bounds = new SKRect();
        paint.MeasureText(nombreAlumno, ref bounds);
        float skiaY = ejeY - bounds.Top;
        canvas.DrawText(nombreAlumno, ejeX, skiaY, paint);

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
                TextAlign = SKTextAlign.Center,
                TextSize = capa.FontSize > 0 ? capa.FontSize : 40,
                Typeface = ResolveTypeface()
            };

            // CSS positions from top-left; SkiaSharp DrawText uses baseline.
            // bounds.Top is negative (ascent), subtracting it gives precise baseline Y.
            SKRect bounds = new SKRect();
            paint.MeasureText(texto, ref bounds);
            float skiaY = capa.Y - bounds.Top;
            canvas.DrawText(texto, capa.X, skiaY, paint);
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

        using var pdfStream = new MemoryStream();
        using var pdfDocument = SKDocument.CreatePdf(pdfStream);
        
        using var canvas = pdfDocument.BeginPage(bitmap.Width, bitmap.Height);
        canvas.DrawBitmap(bitmap, 0, 0);
        pdfDocument.EndPage();
        pdfDocument.Close();
        
        return pdfStream.ToArray();
    }

    private static string EnsureTrailingSeparator(string path)
    {
        if (string.IsNullOrEmpty(path)) return path;
        var sep = Path.DirectorySeparatorChar;
        return path.EndsWith(sep) ? path : path + sep;
    }

    /// <summary>
    /// Loads Montserrat-Bold.ttf from wwwroot/fonts/ if available,
    /// otherwise falls back to Arial system font, then SKTypeface.Default.
    /// </summary>
    private SKTypeface ResolveTypeface()
    {
        var fontPath = Path.Combine(_hostEnvironment.WebRootPath, "fonts", "Montserrat-Bold.ttf");
        if (File.Exists(fontPath))
        {
            var tf = SKTypeface.FromFile(fontPath);
            if (tf != null) return tf;
        }

        return SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
               ?? SKTypeface.Default;
    }
}
