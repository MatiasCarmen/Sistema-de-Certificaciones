namespace Inkillay.Certificados.Web.Models.Entities;

public class Curso
{
    public int IdCurso { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Sumilla { get; set; } = string.Empty;
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public int? IdProfesor { get; set; }
    public decimal Costo { get; set; }
    public bool Estado { get; set; }
}
