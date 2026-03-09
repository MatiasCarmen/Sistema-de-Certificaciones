using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Inkillay.Certificados.Web.Services;

namespace Inkillay.Certificados.Tests;

public class CertificadoServiceTests
{
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly CertificadoService _service;
    private readonly string _tempDirectory;

    public CertificadoServiceTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "InkillayTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
        
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockEnvironment.Setup(m => m.WebRootPath).Returns(_tempDirectory);
        
        _service = new CertificadoService(_mockEnvironment.Object);
    }

    [Fact]
    public void GenerarImagenCertificado_DebeLanzarExcepcion_SiLaRutaEsMaliciosa()
    {
        // Arrange (Preparar)
        string nombreImagenMaliciosa = "../../../secretos/config.json";

        // Act (Actuar)
        Action act = () => _service.GenerarImagenCertificado(nombreImagenMaliciosa, "Matias", 100, 100, 20, "#000");

        // Assert (Verificar) - Aquí usamos FluentAssertions
        act.Should().Throw<UnauthorizedAccessException>()
           .WithMessage("Intento de acceso a ruta no permitida");
    }

    [Fact]
    public void GenerarImagenCertificado_DebeLanzarExcepcion_ConPathTraversal()
    {
        // Arrange
        string rutaTraversal = "..\\..\\..\\secreto.txt";

        // Act
        Action act = () => _service.GenerarImagenCertificado(rutaTraversal, "Usuario", 100, 100, 20, "#000");

        // Assert
        act.Should().Throw<UnauthorizedAccessException>()
           .WithMessage("Intento de acceso a ruta no permitida");
    }

    [Fact]
    public void GenerarImagenCertificado_DebeLanzarExcepcion_SiArchivoNoExiste()
    {
        // Arrange - crear la carpeta uploads pero sin archivo
        var uploadsDir = Path.Combine(_tempDirectory, "uploads");
        Directory.CreateDirectory(uploadsDir);
        
        string nombreArchivoInexistente = "imagen_inexistente.jpg";

        // Act
        Action act = () => _service.GenerarImagenCertificado(nombreArchivoInexistente, "Matias", 100, 100, 20, "#000");

        // Assert
        act.Should().Throw<FileNotFoundException>();
    }

    public void Dispose()
    {
        // Limpiar directorio temporal después de las pruebas
        try
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }
        catch
        {
            // Ignorar errores de limpieza
        }
    }
}
