namespace Inkillay.Certificados.Web.Models.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalAlumnos { get; set; }
    public int CursosActivos { get; set; }
    public decimal RecaudacionTotal { get; set; }
    public int CertificadosEmitidos { get; set; }
    public List<RecaudacionMes> GraficoRecaudacion { get; set; } = new();
}

public class RecaudacionMes
{
    public string Mes { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

public class EstadisticasGenerales
{
    public int TotalAlumnos { get; set; }
    public int CursosActivos { get; set; }
    public decimal RecaudacionTotal { get; set; }
    public int CertificadosEmitidos { get; set; }
}
