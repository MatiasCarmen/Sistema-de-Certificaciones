namespace SIGEC.Certificados.Web.Models.ViewModels;

public class ReporteCursoViewModel
{
    public int IdModulo { get; set; }
    public int IdCurso { get; set; }
    public string NombreCurso { get; set; } = string.Empty;
    public int TotalModulodos { get; set; }
    public int TotalPagados { get; set; }
    public int TotalDescargas { get; set; }
    public int TotalAprobados { get; set; }
    public int TotalPendientesPago { get; set; }
    public int TotalCertificadosEmitidos { get; set; }
}
