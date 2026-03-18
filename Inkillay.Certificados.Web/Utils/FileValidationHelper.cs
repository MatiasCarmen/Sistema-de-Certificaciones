namespace SIGEC.Certificados.Web.Utils;

/// <summary>
/// Helper para validar la integridad de archivos usando Magic Numbers (firmas de archivo)
/// </summary>
public static class FileValidationHelper
{
    // Magic Numbers (signatures) para validar el tipo real de archivo
    private static readonly Dictionary<string, byte[][]> MagicNumbers = new()
    {
        // JPEG signatures
        {
            "jpg", new[]
            {
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, // JFIF
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 }, // EXIF
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 }, // SPIFF
                new byte[] { 0xFF, 0xD8, 0xFF, 0xDB }  // Other JPEG
            }
        },
        // PNG signature
        {
            "png", new[]
            {
                new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }
            }
        }
    };

    /// <summary>
    /// Valida si el contenido del archivo coincide con su extensión
    /// </summary>
    public static bool ValidateMagicNumber(byte[] fileContent, string extension)
    {
        if (fileContent == null || fileContent.Length == 0)
            return false;

        extension = extension.ToLower().TrimStart('.');

        if (!MagicNumbers.TryGetValue(extension, out var signatures))
            return false;

        // Verificar si el contenido comienza con alguna de las firmas válidas
        return signatures.Any(signature =>
            fileContent.Length >= signature.Length &&
            fileContent.Take(signature.Length).SequenceEqual(signature)
        );
    }

    /// <summary>
    /// Valida la extensión del archivo
    /// </summary>
    public static bool ValidateExtension(string fileName, string[] allowedExtensions)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        var extension = Path.GetExtension(fileName).ToLower();
        return allowedExtensions.Any(ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Valida el MIME Type del archivo
    /// </summary>
    public static bool ValidateMimeType(string mimeType, string[] allowedMimeTypes)
    {
        if (string.IsNullOrWhiteSpace(mimeType))
            return false;

        return allowedMimeTypes.Any(mime =>
            mime.Equals(mimeType, StringComparison.OrdinalIgnoreCase)
        );
    }

    /// <summary>
    /// Realiza validación completa del archivo (tamaño, extensión, MIME Type, Magic Number)
    /// </summary>
    public static (bool isValid, string errorMessage) ValidateImageFile(
        IFormFile file,
        long maxSizeBytes = 5 * 1024 * 1024)
    {
        // 1. Validar que no sea nulo
        if (file == null || file.Length == 0)
            return (false, "El archivo no puede estar vacío");

        // 2. Validar tamaño
        if (file.Length > maxSizeBytes)
            return (false, $"El archivo excede el tamaño máximo permitido ({maxSizeBytes / (1024 * 1024)} MB)");

        // 3. Validar extensión
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        if (!ValidateExtension(file.FileName, allowedExtensions))
            return (false, "Solo se permiten archivos JPG o PNG");

        // 4. Validar MIME Type
        var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/pjpeg" };
        if (!ValidateMimeType(file.ContentType, allowedMimeTypes))
            return (false, "El tipo MIME del archivo no es válido");

        // 5. Validar Magic Number (firma del archivo)
        try
        {
            using var memoryStream = new MemoryStream();
            file.CopyTo(memoryStream);
            var fileContent = memoryStream.ToArray();
            var extension = Path.GetExtension(file.FileName);

            if (!ValidateMagicNumber(fileContent, extension))
                return (false, "La firma del archivo no corresponde con su extensión (posible archivo malicioso)");
        }
        catch (Exception ex)
        {
            return (false, $"Error al validar el archivo: {ex.Message}");
        }

        return (true, string.Empty);
    }
}
