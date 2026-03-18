namespace SIGEC.Certificados.Web.Models.Entities;

public class Matricula : AuditoriaBase
{
    public int IdMatricula { get; set; } // Antes IdModuloAlumno
    public int IdModulo { get; set; }
    public int IdUsuario { get; set; } // El ID del alumno
    
    // Datos académicos
    public decimal? NotaFinal { get; set; }
    public char EstadoAlumno { get; set; } = 'E'; // E=En curso, A=Aprobado, D=Desaprobado
    
    // Props de apoyo (llenadas por los SPs con JOINs)
    public string NombreAlumno { get; set; } = string.Empty;
    public string NombreCurso { get; set; } = string.Empty;
    public string EdicionCurso { get; set; } = string.Empty;
    public decimal TotalPagado { get; set; }
    public decimal CostoCurso { get; set; }
    public bool Aprobado => EstadoAlumno == 'A';
}
