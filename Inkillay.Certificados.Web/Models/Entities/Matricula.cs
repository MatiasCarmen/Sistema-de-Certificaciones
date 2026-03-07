namespace Inkillay.Certificados.Web.Models.Entities;

public class Matricula
{
    public int IdMatricula { get; set; }
    public int IdAlumno { get; set; }
    public int IdCurso { get; set; }
    public DateTime FechaMatricula { get; set; }
    public bool Aprobado { get; set; }
    public decimal TotalPagado { get; set; }
    public decimal CostoCurso { get; set; }
    public string Sumilla { get; set; } = string.Empty;
    // Propiedad de navegacion para Dapper si hacemos Join
    public string NombreAlumno { get; set; } = string.Empty;
    public string NombreCurso { get; set; } = string.Empty;
}
