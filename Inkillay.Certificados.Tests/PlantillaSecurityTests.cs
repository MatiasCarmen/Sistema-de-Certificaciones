using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Inkillay.Certificados.Web.Controllers;
using Inkillay.Certificados.Web.Data.Repositories;
using Inkillay.Certificados.Web.Services;
using Inkillay.Certificados.Web.Utils;
using System.Security.Claims;
using System.Text;

namespace Inkillay.Certificados.Tests;

public class PlantillaSecurityTests
{
    [Fact]
    public async Task Crear_DebeRechazar_SiArchivoEsTextoConExtensionJpg()
    {
        // Arrange: Creamos un archivo de texto pero le ponemos nombre .jpg
        var content = "esto no es una imagen, es un script malicioso";
        var fileName = "virus.jpg";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var fileMock = new FormFile(stream, 0, stream.Length, "imagen", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain" // Contenido de texto, no imagen
        };

        // Act: Validar el archivo con FileValidationHelper
        var (isValid, errorMessage) = FileValidationHelper.ValidateImageFile(fileMock);

        // Assert: Debe rechazarse porque el MIME Type es "text/plain"
        isValid.Should().BeFalse("El archivo debe ser rechazado cuando ContentType es text/plain");
        errorMessage.Should().Contain("tipo MIME");
    }

    [Fact]
    public async Task Crear_DebeRechazar_SiMagicNumberNoCoincide()
    {
        // Arrange: Archivo de texto con nombre .png
        var fakeContent = "FAKE PNG DATA - Esto no es una imagen PNG válida";
        var fileName = "fake_image.png";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(fakeContent));
        var fileMock = new FormFile(stream, 0, stream.Length, "imagen", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };

        // Act: Validar el archivo
        var (isValid, errorMessage) = FileValidationHelper.ValidateImageFile(fileMock);

        // Assert: Debe fallar porque no tiene la firma PNG válida
        isValid.Should().BeFalse("El archivo debe ser rechazado cuando no tiene Magic Number válido");
        errorMessage.Should().Contain("firma del archivo");
    }

    [Fact]
    public void ValidateMagicNumber_DebeAceptar_JpegValido()
    {
        // Arrange: JPEG válido con firma JFIF
        byte[] jpegContent = new byte[]
        {
            0xFF, 0xD8, 0xFF, 0xE0, // JPEG JFIF signature
            0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01 // JFIF header
        };

        // Act
        var isValid = FileValidationHelper.ValidateMagicNumber(jpegContent, ".jpg");

        // Assert
        isValid.Should().BeTrue("Debe reconocer un JPEG válido con firma JFIF");
    }

    [Fact]
    public void ValidateMagicNumber_DebeAceptar_PngValido()
    {
        // Arrange: PNG válido con firma correcta
        byte[] pngContent = new byte[]
        {
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, // PNG signature
            0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52  // IHDR chunk
        };

        // Act
        var isValid = FileValidationHelper.ValidateMagicNumber(pngContent, ".png");

        // Assert
        isValid.Should().BeTrue("Debe reconocer un PNG válido");
    }

    [Fact]
    public void ValidateMagicNumber_DebeRechazar_ContenidoMalicioso()
    {
        // Arrange: Contenido ejecutable (pseudo-código malicioso)
        byte[] executableContent = new byte[] { 0x4D, 0x5A, 0x90, 0x00 }; // PE/MSDOS executable header

        // Act
        var isValidJpg = FileValidationHelper.ValidateMagicNumber(executableContent, ".jpg");
        var isValidPng = FileValidationHelper.ValidateMagicNumber(executableContent, ".png");

        // Assert
        isValidJpg.Should().BeFalse("Debe rechazar ejecutables incluso si se declaran como .jpg");
        isValidPng.Should().BeFalse("Debe rechazar ejecutables incluso si se declaran como .png");
    }

    [Fact]
    public void ValidateMimeType_DebeAceptar_MimeTypesValidos()
    {
        // Arrange
        var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/pjpeg" };

        // Act & Assert
        FileValidationHelper.ValidateMimeType("image/jpeg", allowedMimeTypes).Should().BeTrue();
        FileValidationHelper.ValidateMimeType("image/png", allowedMimeTypes).Should().BeTrue();
        FileValidationHelper.ValidateMimeType("image/pjpeg", allowedMimeTypes).Should().BeTrue();
    }

    [Fact]
    public void ValidateMimeType_DebeRechazar_MimeTypeInvalido()
    {
        // Arrange
        var allowedMimeTypes = new[] { "image/jpeg", "image/png" };

        // Act & Assert
        FileValidationHelper.ValidateMimeType("text/plain", allowedMimeTypes).Should().BeFalse();
        FileValidationHelper.ValidateMimeType("application/x-msdownload", allowedMimeTypes).Should().BeFalse();
        FileValidationHelper.ValidateMimeType("application/zip", allowedMimeTypes).Should().BeFalse();
    }

    [Fact]
    public void ValidateExtension_DebeAceptar_ExtensionesValidas()
    {
        // Arrange
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

        // Act & Assert
        FileValidationHelper.ValidateExtension("imagen.jpg", allowedExtensions).Should().BeTrue();
        FileValidationHelper.ValidateExtension("foto.jpeg", allowedExtensions).Should().BeTrue();
        FileValidationHelper.ValidateExtension("picture.png", allowedExtensions).Should().BeTrue();
    }

    [Fact]
    public void ValidateExtension_DebeRechazar_ExtensionesInvalidas()
    {
        // Arrange
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

        // Act & Assert
        FileValidationHelper.ValidateExtension("archivo.exe", allowedExtensions).Should().BeFalse();
        FileValidationHelper.ValidateExtension("script.php", allowedExtensions).Should().BeFalse();
        FileValidationHelper.ValidateExtension("documento.pdf", allowedExtensions).Should().BeFalse();
    }

    [Fact]
    public void ValidateImageFile_DebeRechazar_ArchivoNulo()
    {
        // Act
        var (isValid, errorMessage) = FileValidationHelper.ValidateImageFile(null);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("vacío");
    }

    [Fact]
    public void ValidateImageFile_DebeRechazar_ArchivoMuyGrande()
    {
        // Arrange: Archivo de 10MB (más que el máximo de 5MB)
        var largeContent = new byte[10 * 1024 * 1024];
        var stream = new MemoryStream(largeContent);
        var fileMock = new FormFile(stream, 0, stream.Length, "imagen", "large.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };

        // Act
        var (isValid, errorMessage) = FileValidationHelper.ValidateImageFile(fileMock);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("tamaño");
    }

    [Fact]
    public void ValidateImageFile_DebeRechazar_ExtensionNoPermitida()
    {
        // Arrange
        var content = "some content";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var fileMock = new FormFile(stream, 0, stream.Length, "imagen", "document.pdf")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf"
        };

        // Act
        var (isValid, errorMessage) = FileValidationHelper.ValidateImageFile(fileMock);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("JPG", "PNG");
    }
}
