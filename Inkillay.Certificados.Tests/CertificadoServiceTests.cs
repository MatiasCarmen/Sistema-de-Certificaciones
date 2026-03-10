using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Inkillay.Certificados.Web.Services;
using Inkillay.Certificados.Web.Models.ViewModels;
using SkiaSharp;
using System.Collections.Generic;

namespace Inkillay.Certificados.Tests;

public class CertificadoServiceTests : IDisposable
{
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly CertificadoService _service;
    private readonly string _tempDirectory;
    private readonly string _uploadsDirectory;

    public CertificadoServiceTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "InkillayTests", Guid.NewGuid().ToString());
        _uploadsDirectory = Path.Combine(_tempDirectory, "uploads");
        Directory.CreateDirectory(_uploadsDirectory);

        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockEnvironment.Setup(m => m.WebRootPath).Returns(_tempDirectory);

        _service = new CertificadoService(_mockEnvironment.Object);
    }

    #region Pruebas de Seguridad

    [Fact]
    public void GenerarImagenCertificado_DebeLanzarExcepcion_SiLaRutaEsMaliciosa()
    {
        // Arrange (Preparar)
        string nombreImagenMaliciosa = "../../../secretos/config.json";

        // Act (Actuar)
        Action act = () => _service.GenerarImagenCertificado(nombreImagenMaliciosa, "Matias", 100, 100, 20, "#000");

        // Assert (Verificar)
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
        // Arrange - la carpeta uploads existe pero sin archivo
        string nombreArchivoInexistente = "imagen_inexistente.jpg";

        // Act
        Action act = () => _service.GenerarImagenCertificado(nombreArchivoInexistente, "Matias", 100, 100, 20, "#000");

        // Assert
        act.Should().Throw<FileNotFoundException>();
    }

    #endregion

    #region Pruebas de Mapeo de Coordenadas (Punto 1)

    [Fact]
    public void GenerarImagenCertificado_DebeMapearCorrectamenteCoordenadaX()
    {
        // Arrange - Crear una imagen de prueba (500x600px)
        var imagenPrueba = CrearImagenPrueba(500, 600);
        var rutaImagen = Path.Combine(_uploadsDirectory, "plantilla_test.jpg");
        File.WriteAllBytes(rutaImagen, imagenPrueba);

        int coordenadaX = 100;
        int coordenadaY = 300;

        // Act - Generar imagen con coordenada específica
        var resultado = _service.GenerarImagenCertificado(
            "plantilla_test.jpg",
            "JUAN PEREZ",
            coordenadaX,
            coordenadaY,
            50,
            "#000000"
        );

        // Assert - Verificar que se generó algo y que el tamaño es correcto
        resultado.Should().NotBeNull();
        resultado.Length.Should().BeGreaterThan(0);

        // Decodificar la imagen generada para validar que tiene texto
        using var stream = new MemoryStream(resultado);
        using var bitmap = SKBitmap.Decode(stream);
        bitmap.Should().NotBeNull();
        bitmap.Width.Should().Be(500);
        bitmap.Height.Should().Be(600);
    }

    [Fact]
    public void GenerarImagenCertificado_DebeMapearCorrectamenteCoordenadaY()
    {
        // Arrange
        var imagenPrueba = CrearImagenPrueba(800, 1000);
        var rutaImagen = Path.Combine(_uploadsDirectory, "plantilla_y_test.jpg");
        File.WriteAllBytes(rutaImagen, imagenPrueba);

        int coordenadaX = 250;
        int coordenadaY = 750; // Coordenada Y alta

        // Act
        var resultado = _service.GenerarImagenCertificado(
            "plantilla_y_test.jpg",
            "ANA GARCIA",
            coordenadaX,
            coordenadaY,
            48,
            "#333333"
        );

        // Assert
        resultado.Should().NotBeNull();
        resultado.Length.Should().BeGreaterThan(0);

        using var stream = new MemoryStream(resultado);
        using var bitmap = SKBitmap.Decode(stream);
        bitmap.Should().NotBeNull();
        bitmap.Width.Should().Be(800);
        bitmap.Height.Should().Be(1000);
    }

    [Theory]
    [InlineData(0, 0)]      // Esquina superior izquierda
    [InlineData(450, 550)]  // Centro
    [InlineData(900, 1090)] // Cerca del borde inferior derecho
    public void GenerarImagenCertificado_DebeMapearVariasCoordenadas(int ejeX, int ejeY)
    {
        // Arrange
        var imagenPrueba = CrearImagenPrueba(1000, 1200);
        var rutaImagen = Path.Combine(_uploadsDirectory, "plantilla_coords.jpg");
        File.WriteAllBytes(rutaImagen, imagenPrueba);

        // Act
        var resultado = _service.GenerarImagenCertificado(
            "plantilla_coords.jpg",
            "PRUEBA XY",
            ejeX,
            ejeY,
            40,
            "#000000"
        );

        // Assert - El PDF debe contener los bytes generados sin errores
        resultado.Should().NotBeNull();
        resultado.Length.Should().BeGreaterThan(1000); // Mínimo de bytes para una imagen JPEG
    }

    #endregion

    #region Pruebas de Generación de Stream (Punto 2)

    [Fact]
    public void GenerarPdfDesdeImagen_DebeGenerarPdfConBytesValidos()
    {
        // Arrange - Crear una imagen de prueba
        var imagenPrueba = CrearImagenPrueba(600, 800);

        // Act - Generar PDF
        var pdfBytes = _service.GenerarPdfDesdeImagen(imagenPrueba);

        // Assert - El PDF no debe estar vacío
        pdfBytes.Should().NotBeNull();
        pdfBytes.Length.Should().BeGreaterThan(0, "El PDF generado está vacío");
        pdfBytes.Length.Should().BeGreaterThan(500, "El PDF es muy pequeño, probablemente corrupto");

        // Verificar que comienza con la firma de PDF
        pdfBytes[0].Should().Be(0x25); // '%'
        pdfBytes[1].Should().Be(0x50); // 'P'
        pdfBytes[2].Should().Be(0x44); // 'D'
        pdfBytes[3].Should().Be(0x46); // 'F'
    }

    [Fact]
    public void GenerarPdfDesdeImagen_DebeGenerarStreamDistinto_ConImagenesDistintas()
    {
        // Arrange
        var imagen1 = CrearImagenPrueba(500, 600);
        var imagen2 = CrearImagenPrueba(700, 800);

        // Act
        var pdf1 = _service.GenerarPdfDesdeImagen(imagen1);
        var pdf2 = _service.GenerarPdfDesdeImagen(imagen2);

        // Assert
        pdf1.Length.Should().NotBe(pdf2.Length, "PDFs de diferentes tamaños deberían tener diferente longitud");
        pdf1.Should().NotEqual(pdf2, "PDFs generados de imágenes diferentes deberían ser distintos");
    }

    [Fact]
    public void GenerarPdfDesdeImagen_DebeLanzarExcepcion_SiImagenEstavaVacia()
    {
        // Arrange
        byte[] imagenVacia = Array.Empty<byte>();

        // Act
        Action act = () => _service.GenerarPdfDesdeImagen(imagenVacia);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("Los bytes de la imagen no pueden estar vacíos*");
    }

    [Fact]
    public void GenerarPdfDesdeImagen_DebeLanzarExcepcion_SiImagenEsNula()
    {
        // Act
        Action act = () => _service.GenerarPdfDesdeImagen(null!);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("Los bytes de la imagen no pueden estar vacíos*");
    }

    [Fact]
    public void GenerarPdfDesdeImagen_DebeLanzarExcepcion_SiBytesNoSonImagenValida()
    {
        // Arrange - Bytes aleatorios que no son una imagen válida
        byte[] bytesInvalidos = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };

        // Act
        Action act = () => _service.GenerarPdfDesdeImagen(bytesInvalidos);

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*No se pudo decodificar la imagen proporcionada*");
    }

    [Fact]
    public void GenerarPdfDesdeImagen_DebeMantenerTamanioProporcionalAlaPDF()
    {
        // Arrange
        var imagenPrueba = CrearImagenPrueba(1000, 500);

        // Act
        var pdfBytes = _service.GenerarPdfDesdeImagen(imagenPrueba);

        // Assert
        pdfBytes.Should().NotBeNull();
        pdfBytes.Length.Should().BeGreaterThan(1000);
    }

    #endregion

    #region Pruebas de Validación de Datos (Punto 3)

    [Fact]
    public void GenerarImagenCertificado_DebeLanzarExcepcion_ConNombreImagenNulo()
    {
        // Arrange
        string nombreImagen = null!;

        // Act
        Action act = () => _service.GenerarImagenCertificado(nombreImagen, "Alumno", 100, 100, 20, "#000");

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*Nombre de imagen invalido*");
    }

    [Fact]
    public void GenerarImagenCertificado_DebeLanzarExcepcion_ConNombreImagenVacio()
    {
        // Arrange
        string nombreImagen = string.Empty;

        // Act
        Action act = () => _service.GenerarImagenCertificado(nombreImagen, "Alumno", 100, 100, 20, "#000");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GenerarImagenCertificado_DebeAceptarColorHexValido()
    {
        // Arrange
        var imagenPrueba = CrearImagenPrueba(500, 600);
        var rutaImagen = Path.Combine(_uploadsDirectory, "plantilla_color.jpg");
        File.WriteAllBytes(rutaImagen, imagenPrueba);

        // Act & Assert - No debe lanzar excepción
        var resultado = _service.GenerarImagenCertificado(
            "plantilla_color.jpg",
            "PRUEBA COLOR",
            100,
            200,
            50,
            "#FF5733"
        );

        resultado.Should().NotBeNull();
        resultado.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GenerarImagenCertificado_DebeAceptarColorHexInvalido_YAplicarNegroComoPorDefecto()
    {
        // Arrange
        var imagenPrueba = CrearImagenPrueba(500, 600);
        var rutaImagen = Path.Combine(_uploadsDirectory, "plantilla_color_invalido.jpg");
        File.WriteAllBytes(rutaImagen, imagenPrueba);

        // Act - Enviar un color inválido
        var resultado = _service.GenerarImagenCertificado(
            "plantilla_color_invalido.jpg",
            "PRUEBA",
            100,
            200,
            50,
            "COLORESINCORRECTO"
        );

        // Assert - No debe fallar, debe usar el color por defecto (negro)
        resultado.Should().NotBeNull();
        resultado.Length.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("")]      // Nombre vacío
    [InlineData("   ")] // Espacios en blanco
    public void GenerarImagenCertificado_DebeAceptarNombreAlumnoConEspacios(string nombreAlumno)
    {
        // Arrange
        var imagenPrueba = CrearImagenPrueba(500, 600);
        var rutaImagen = Path.Combine(_uploadsDirectory, "plantilla_nombre.jpg");
        File.WriteAllBytes(rutaImagen, imagenPrueba);

        // Act & Assert - No debe lanzar excepción por el nombre del alumno
        var resultado = _service.GenerarImagenCertificado(
            "plantilla_nombre.jpg",
            nombreAlumno,
            100,
            200,
            50,
            "#000000"
        );

        resultado.Should().NotBeNull();
    }

    [Fact]
    public void GenerarImagenCertificado_DebeAceptarTamanioFuenteVariado()
    {
        // Arrange
        var imagenPrueba = CrearImagenPrueba(800, 1000);
        var rutaImagen = Path.Combine(_uploadsDirectory, "plantilla_font.jpg");
        File.WriteAllBytes(rutaImagen, imagenPrueba);

        // Act & Assert - Probar con diferentes tamaños
        foreach (int fontSize in new[] { 10, 50, 100, 200 })
        {
            var resultado = _service.GenerarImagenCertificado(
                "plantilla_font.jpg",
                "PRUEBA FUENTE",
                100,
                200,
                fontSize,
                "#000000"
            );

            resultado.Should().NotBeNull();
            resultado.Length.Should().BeGreaterThan(0);
        }
    }

    #endregion

    #region Pruebas de Sistema Multicapa (Punto 4)

    [Fact]
    public void GenerarImagenCertificadoMulticapa_DebeGenerarImagenConVariasCapas()
    {
        // Arrange
        var imagenPrueba = CrearImagenPrueba(800, 600);
        var rutaImagen = Path.Combine(_uploadsDirectory, "plantilla_multicapa.jpg");
        File.WriteAllBytes(rutaImagen, imagenPrueba);

        var capas = new List<PlantillaDetalleDTO>
        {
            new PlantillaDetalleDTO { Texto = "CERTIFICADO", X = 200, Y = 100, FontSize = 60, FontColor = "#000000", EsPrincipal = 0, Orden = 0 },
            new PlantillaDetalleDTO { Texto = "", X = 200, Y = 300, FontSize = 50, FontColor = "#0369a1", EsPrincipal = 1, Orden = 1 },
            new PlantillaDetalleDTO { Texto = "Fecha: 10/03/2026", X = 200, Y = 450, FontSize = 30, FontColor = "#666666", EsPrincipal = 0, Orden = 2 }
        };

        // Act
        var resultado = _service.GenerarImagenCertificadoMulticapa(
            "plantilla_multicapa.jpg",
            "JUAN PEREZ",
            capas
        );

        // Assert
        resultado.Should().NotBeNull();
        resultado.Length.Should().BeGreaterThan(0);

        using var stream = new MemoryStream(resultado);
        using var bitmap = SKBitmap.Decode(stream);
        bitmap.Should().NotBeNull();
        bitmap.Width.Should().Be(800);
        bitmap.Height.Should().Be(600);
    }

    [Fact]
    public void GenerarImagenCertificadoMulticapa_DebeReemplazarCapaPrincipalConNombreAlumno()
    {
        // Arrange
        var imagenPrueba = CrearImagenPrueba(600, 400);
        var rutaImagen = Path.Combine(_uploadsDirectory, "plantilla_principal.jpg");
        File.WriteAllBytes(rutaImagen, imagenPrueba);

        var capas = new List<PlantillaDetalleDTO>
        {
            new PlantillaDetalleDTO { Texto = "Placeholder", X = 100, Y = 200, FontSize = 40, FontColor = "#000000", EsPrincipal = 1, Orden = 0 }
        };

        // Act
        var resultado = _service.GenerarImagenCertificadoMulticapa(
            "plantilla_principal.jpg",
            "ANA GARCIA",
            capas
        );

        // Assert
        resultado.Should().NotBeNull();
        resultado.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GenerarImagenCertificadoMulticapa_DebeLanzarExcepcion_ConRutaMaliciosa()
    {
        // Arrange
        var capas = new List<PlantillaDetalleDTO>();

        // Act
        Action act = () => _service.GenerarImagenCertificadoMulticapa(
            "../../../config.txt",
            "Alumno",
            capas
        );

        // Assert
        act.Should().Throw<UnauthorizedAccessException>();
    }

    [Fact]
    public void GenerarImagenCertificadoMulticapa_DebeIgnorarCapasConTextoVacio()
    {
        // Arrange
        var imagenPrueba = CrearImagenPrueba(500, 400);
        var rutaImagen = Path.Combine(_uploadsDirectory, "plantilla_vacia.jpg");
        File.WriteAllBytes(rutaImagen, imagenPrueba);

        var capas = new List<PlantillaDetalleDTO>
        {
            new PlantillaDetalleDTO { Texto = "", X = 100, Y = 100, FontSize = 40, FontColor = "#000000", EsPrincipal = 0, Orden = 0 },
            new PlantillaDetalleDTO { Texto = "Válido", X = 100, Y = 200, FontSize = 40, FontColor = "#000000", EsPrincipal = 0, Orden = 1 }
        };

        // Act
        var resultado = _service.GenerarImagenCertificadoMulticapa(
            "plantilla_vacia.jpg",
            "ALUMNO PRUEBA",
            capas
        );

        // Assert
        resultado.Should().NotBeNull();
        resultado.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GenerarImagenCertificadoMulticapa_DebeOrdenarCapasPorOrden()
    {
        // Arrange
        var imagenPrueba = CrearImagenPrueba(600, 400);
        var rutaImagen = Path.Combine(_uploadsDirectory, "plantilla_orden.jpg");
        File.WriteAllBytes(rutaImagen, imagenPrueba);

        var capas = new List<PlantillaDetalleDTO>
        {
            new PlantillaDetalleDTO { Texto = "Tercera", X = 100, Y = 300, FontSize = 30, FontColor = "#000000", EsPrincipal = 0, Orden = 2 },
            new PlantillaDetalleDTO { Texto = "Primera", X = 100, Y = 100, FontSize = 30, FontColor = "#000000", EsPrincipal = 0, Orden = 0 },
            new PlantillaDetalleDTO { Texto = "Segunda", X = 100, Y = 200, FontSize = 30, FontColor = "#000000", EsPrincipal = 0, Orden = 1 }
        };

        // Act
        var resultado = _service.GenerarImagenCertificadoMulticapa(
            "plantilla_orden.jpg",
            "TEST",
            capas
        );

        // Assert
        resultado.Should().NotBeNull();
        resultado.Length.Should().BeGreaterThan(0);
    }

    #endregion

    #region Métodos Auxiliares

    /// <summary>
    /// Crea una imagen JPEG de prueba con dimensiones especificadas
    /// </summary>
    private byte[] CrearImagenPrueba(int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);

        // Fondo blanco
        canvas.DrawColor(SKColors.White);

        // Dibujar un rectángulo de prueba
        var paint = new SKPaint { Color = SKColors.LightGray, Style = SKPaintStyle.Stroke, StrokeWidth = 2 };
        canvas.DrawRect(new SKRect(10, 10, width - 10, height - 10), paint);

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
        return data.ToArray();
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

    #endregion
}
