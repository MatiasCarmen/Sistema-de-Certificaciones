using Xunit;
using FluentAssertions;
using Inkillay.Certificados.Web.Models.ViewModels;

namespace Inkillay.Certificados.Tests;

public class AdminDashboardTests
{
    [Fact]
    public void AdminDashboard_DebeInicializar_ConValoresPorDefecto()
    {
        // Arrange & Act
        var dashboard = new AdminDashboardViewModel();

        // Assert
        dashboard.TotalAlumnos.Should().Be(0);
        dashboard.CursosActivos.Should().Be(0);
        dashboard.RecaudacionTotal.Should().Be(0);
        dashboard.CertificadosEmitidos.Should().Be(0);
        dashboard.GraficoRecaudacion.Should().BeEmpty();
    }

    [Fact]
    public void AdminDashboard_DebeGuardar_EstadisticasCorectamente()
    {
        // Arrange
        var dashboard = new AdminDashboardViewModel
        {
            TotalAlumnos = 150,
            CursosActivos = 5,
            RecaudacionTotal = 50000.00m,
            CertificadosEmitidos = 320
        };

        // Act & Assert
        dashboard.TotalAlumnos.Should().Be(150);
        dashboard.CursosActivos.Should().Be(5);
        dashboard.RecaudacionTotal.Should().Be(50000.00m);
        dashboard.CertificadosEmitidos.Should().Be(320);
    }

    [Fact]
    public void AdminDashboard_DebeAñadir_DatosDeRecaudacion()
    {
        // Arrange
        var dashboard = new AdminDashboardViewModel();
        var recaudacion1 = new RecaudacionMes { Mes = "Ene 2024", Total = 10000 };
        var recaudacion2 = new RecaudacionMes { Mes = "Feb 2024", Total = 12000 };

        // Act
        dashboard.GraficoRecaudacion.Add(recaudacion1);
        dashboard.GraficoRecaudacion.Add(recaudacion2);

        // Assert
        dashboard.GraficoRecaudacion.Should().HaveCount(2);
        dashboard.GraficoRecaudacion[0].Mes.Should().Be("Ene 2024");
        dashboard.GraficoRecaudacion[0].Total.Should().Be(10000);
        dashboard.GraficoRecaudacion[1].Mes.Should().Be("Feb 2024");
        dashboard.GraficoRecaudacion[1].Total.Should().Be(12000);
    }

    [Fact]
    public void AdminDashboard_DebeCalcular_RecaudacionTotal()
    {
        // Arrange
        var dashboard = new AdminDashboardViewModel();
        dashboard.GraficoRecaudacion.Add(new RecaudacionMes { Mes = "Ene", Total = 10000 });
        dashboard.GraficoRecaudacion.Add(new RecaudacionMes { Mes = "Feb", Total = 15000 });
        dashboard.GraficoRecaudacion.Add(new RecaudacionMes { Mes = "Mar", Total = 12000 });

        // Act
        var totalCalculado = dashboard.GraficoRecaudacion.Sum(x => x.Total);

        // Assert
        totalCalculado.Should().Be(37000);
    }

    [Fact]
    public void RecaudacionMes_DebeGuardar_MesYTotal()
    {
        // Arrange & Act
        var recaudacion = new RecaudacionMes
        {
            Mes = "Enero 2024",
            Total = 25000.50m
        };

        // Assert
        recaudacion.Mes.Should().Be("Enero 2024");
        recaudacion.Total.Should().Be(25000.50m);
    }

    [Fact]
    public void AdminDashboard_DebeValidar_AlgunasMétricas()
    {
        // Arrange
        var dashboard = new AdminDashboardViewModel
        {
            TotalAlumnos = 100,
            CursosActivos = 3,
            RecaudacionTotal = 30000,
            CertificadosEmitidos = 150
        };

        // Act & Assert
        // Validaciones lógicas
        dashboard.TotalAlumnos.Should().BeGreaterThanOrEqualTo(0);
        dashboard.CursosActivos.Should().BeGreaterThanOrEqualTo(0);
        dashboard.RecaudacionTotal.Should().BeGreaterThanOrEqualTo(0);
        dashboard.CertificadosEmitidos.Should().BeGreaterThanOrEqualTo(0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(1000)]
    public void AdminDashboard_DebeAceptar_RecaudacionValida(decimal monto)
    {
        // Arrange & Act
        var dashboard = new AdminDashboardViewModel { RecaudacionTotal = monto };

        // Assert
        dashboard.RecaudacionTotal.Should().Be(monto);
        dashboard.RecaudacionTotal.Should().BeGreaterThanOrEqualTo(0);
    }
}
