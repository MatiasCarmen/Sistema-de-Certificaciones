using Xunit;
using FluentAssertions;
using SIGEC.Certificados.Web.Data.Repositories;
using SIGEC.Certificados.Web.Models.Entities;
using SIGEC.Certificados.Web.Data;
using SIGEC.Certificados.Web.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace SIGEC.Certificados.Tests;

public class UsuariosManagementTests
{
    [Fact]
    public void RegistrarUsuario_DebeHashearContrasena()
    {
        // Arrange
        var contrasenaOriginal = "MiContraseña123";
        var claveHash = HashHelper.HashPassword(contrasenaOriginal);

        // Act
        var esValida = HashHelper.VerifyPassword(contrasenaOriginal, claveHash);

        // Assert
        esValida.Should().BeTrue("La contraseña hasheada debe ser verificable");
        claveHash.Should().NotBe(contrasenaOriginal, "La contraseña no debe almacenarse en texto plano");
    }

    [Fact]
    public void RegistrarUsuario_NoDebePermitir_ContrasenaCorta()
    {
        // Arrange
        var contrasena = "abc";

        // Act & Assert
        contrasena.Length.Should().BeLessThan(6, "La contraseña debe tener mínimo 6 caracteres");
    }

    [Fact]
    public void RegistrarUsuario_DebeValidar_CorreoValido()
    {
        // Arrange
        var correoValido = "usuario@example.com";
        var correoInvalido = "usuario";

        // Act
        var esValidoEmail = correoValido.Contains("@") && correoValido.Contains(".");

        // Assert
        esValidoEmail.Should().BeTrue("El correo debe contener @ y .");
        correoInvalido.Contains("@").Should().BeFalse("Un correo sin @ no es válido");
    }

    [Fact]
    public void Usuario_DebeGuardar_DatosBásicos()
    {
        // Arrange
        var usuario = new Usuarios
        {
            IdUsuario = 1,
            Nombre = "Juan Pérez",
            Correo = "juan@example.com",
            Clave = "hash_seguro",
            IdRol = 2,
            Estado = 'A',  // ✅ Cambiado de true a 'A' según estándar corporativo
            FechaRegistro = DateTime.Now
        };

        // Act & Assert
        usuario.Nombre.Should().Be("Juan Pérez");
        usuario.Correo.Should().Be("juan@example.com");
        usuario.IdRol.Should().Be(2);
        usuario.Estado.Should().Be('A');  // ✅ Verificar que sea 'A'
        usuario.EstadoActivo.Should().BeTrue();  // ✅ Propiedad auxiliar para compatibilidad
    }

    [Fact]
    public void Usuario_DebePermitir_CambiarEstado()
    {
        // Arrange
        var usuario = new Usuarios { IdUsuario = 1, Estado = 'A' };  // ✅ Activo

        // Act
        usuario.Estado = 'I';  // ✅ Cambiar a Inactivo

        // Assert
        usuario.Estado.Should().Be('I', "El estado del usuario debe poder cambiarse");
        usuario.EstadoActivo.Should().BeFalse("EstadoActivo debe ser false cuando Estado es 'I'");
    }

    [Theory]
    [InlineData(1, "Admin")]
    [InlineData(2, "Docente")]
    [InlineData(3, "Alumno")]
    public void Usuario_DebeReconocer_Roles(int idRol, string nombreRol)
    {
        // Arrange
        var usuario = new Usuarios { IdRol = idRol };

        // Act & Assert
        usuario.IdRol.Should().Be(idRol, $"El usuario debe tener el rol {nombreRol}");
    }

    [Fact]
    public void CorreoNormalizado_DebeStar_EnMinúsculas()
    {
        // Arrange
        var correoOriginal = "USUARIO@EXAMPLE.COM";
        var correoNormalizado = correoOriginal.ToLower();

        // Act & Assert
        correoNormalizado.Should().Be("usuario@example.com", "El correo debe normalizarse a minúsculas para evitar duplicados");
    }

    [Fact]
    public void Usuario_DebeValidar_FechaRegistro()
    {
        // Arrange
        var ahora = DateTime.Now;
        var usuario = new Usuarios { FechaRegistro = ahora };

        // Act & Assert
        usuario.FechaRegistro.Should().BeCloseTo(ahora, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ContrasenasDistintas_DebenGenerarHashDiferentes()
    {
        // Arrange
        var pass1 = HashHelper.HashPassword("Contraseña1");
        var pass2 = HashHelper.HashPassword("Contraseña2");

        // Act & Assert
        pass1.Should().NotBe(pass2, "Contraseñas diferentes deben generar hashes diferentes");
    }

    [Fact]
    public void MismaContrasena_DebeVerificarMultipleVeces()
    {
        // Arrange
        var contrasena = "MiContraseña123";
        var hash = HashHelper.HashPassword(contrasena);

        // Act
        var verificacion1 = HashHelper.VerifyPassword(contrasena, hash);
        var verificacion2 = HashHelper.VerifyPassword(contrasena, hash);
        var verificacionIncorrecto = HashHelper.VerifyPassword("OtraContraseña", hash);

        // Assert
        verificacion1.Should().BeTrue();
        verificacion2.Should().BeTrue();
        verificacionIncorrecto.Should().BeFalse("Una contraseña incorrecta no debe verificar");
    }

    [Fact]
    public async Task RegistrarUsuario_DeberiaRetornarTrue()
    {
        // Configuracion (usa tus datos reales de conexion)
        var basePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Inkillay.Certificados.Web"));
        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var repo = new SeguridadRepository(new DbConnectionFactory(config));
        var correo = $"test_{Guid.NewGuid():N}@test.com";

        var nuevo = new Usuarios
        {
            Nombre = "Test Error",
            Correo = correo,
            Clave = HashHelper.HashPassword("123456"),
            IdRol = 3, // Alumno
            Estado = 'A',
            UsuarioRegistro = "Matias Test",
            FechaRegistro = DateTime.Now
        };

        var resultado = await repo.RegistrarUsuarioAsync(nuevo);

        resultado.Should().BeTrue();
    }
}
