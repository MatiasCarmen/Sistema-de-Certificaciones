namespace SIGEC.Certificados.Web.Models.ViewModels;

public class DocenteDashboardViewModel
{
    public int TotalModulos { get; set; }
    public int ModulosActivos { get; set; }
    public int TotalAlumnos { get; set; }
    public decimal RecaudacionTotal { get; set; }
}

public class AlumnoDashboardViewModel
{
    public int TotalCursos { get; set; }
    public int CursosActivos { get; set; }
    public int CertificadosObtenidos { get; set; }
    public decimal TotalPagado { get; set; }
}
