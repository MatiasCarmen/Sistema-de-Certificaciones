namespace SIGEC.Certificados.Web.Models.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalAlumnos { get; set; }
    public int CursosActivos { get; set; }
    public decimal RecaudacionTotal { get; set; }
    public int CertificadosEmitidos { get; set; }
    public int TotalMatriculas { get; set; }
    public int ModulosActivos { get; set; }
    public List<RecaudacionMes> GraficoRecaudacion { get; set; } = new();
    public List<ActividadReciente> ActividadReciente { get; set; } = new();
}

public class RecaudacionMes
{
    public string Mes { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

public class ActividadReciente
{
    public string Accion { get; set; } = string.Empty;
    public string Detalles { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string TiempoRelativo => ObtenerTiempoRelativo(Fecha);

    private string ObtenerTiempoRelativo(DateTime fecha)
    {
        var diferencia = DateTime.Now - fecha;
        if (diferencia.TotalMinutes < 1) return "Hace segundos";
        if (diferencia.TotalMinutes < 60) return $"Hace {(int)diferencia.TotalMinutes} min";
        if (diferencia.TotalHours < 24) return $"Hace {(int)diferencia.TotalHours} h";
        return fecha.ToString("dd/MM/yyyy");
    }
}

public class EstadisticasGenerales
{
    public int TotalAlumnos { get; set; }
    public int CursosActivos { get; set; }
    public decimal RecaudacionTotal { get; set; }
    public int CertificadosEmitidos { get; set; }
}
