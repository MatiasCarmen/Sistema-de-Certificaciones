namespace Inkillay.Certificados.Web.Models.Entities;

public class ModuloAlumno : AuditoriaBase
{
    public int IdModuloAlumno { get; set; }
    public int IdModulo { get; set; }
    
    // Alumno o Usuario
    public int IdUsuario { get; set; }

    /// <summary>
    /// Nota final del estudiante en el curso
    /// </summary>
    public decimal? NotaFinal { get; set; }

    /// <summary>
    /// Estado del alumno en el módulo. Ej: 'A'probado, 'R'eprobado, 'C'ancelado, 'E'n progreso
    /// </summary>
    public char EstadoAlumno { get; set; } = 'E';

    /// <summary>
    /// Estado del registro (A=Activo, I=Inactivo)
    /// </summary>
    public char Estado { get; set; } = 'A';

    // Propiedades calculadas (relaciones para la vista)
    public string NombreAlumno { get; set; } = string.Empty;
}
