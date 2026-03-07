namespace Inkillay.Certificados.Web.Models.ViewModels;

public class ReporteCursoViewModel
{
    public int IdCurso { get; set; }
    public string NombreCurso { get; set; } = string.Empty;
    public int TotalMatriculados { get; set; }
    public int TotalPagados { get; set; }
    public int TotalDescargas { get; set; }
    public int TotalAprobados { get; set; }
    public int TotalPendientesPago { get; set; }
    public int TotalCertificadosEmitidos { get; set; }
}
