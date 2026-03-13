namespace Inkillay.Certificados.Web.Models.Entities;

/// <summary>
/// Entidad Matrícula según el estándar corporativo actualizado
/// </summary>
public class Matricula : AuditoriaBase
{
    public int IdMatricula { get; set; }
    public int IdAlumno { get; set; }
    public int IdCurso { get; set; }

    /// <summary>
    /// Indica si el alumno aprobó el curso (legacy: aún se usa en la lógica de certificados)
    /// </summary>
    public bool Aprobado { get; set; }

    /// <summary>
    /// Estado de la matrícula: 1=Registrada, 2=EnCurso, 3=Completada, 4=Aprobada, 5=Reprobada, 6=Cancelada
    /// ⚠️ NUEVO CAMPO según estándar corporativo
    /// </summary>
    public int EstadoMatricula { get; set; } = 1; // Por defecto: Registrada

    /// <summary>
    /// Modalidad del cursado: 'P'=Presencial, 'R'=Remoto, 'H'=Híbrido
    /// ⚠️ NUEVO CAMPO según estándar corporativo
    /// </summary>
    public char Modalidad { get; set; } = 'P'; // Por defecto: Presencial

    // Propiedades calculadas (no mapean directamente a BD, se obtienen por JOINs)
    public decimal TotalPagado { get; set; }
    public decimal CostoCurso { get; set; }
    public string Sumilla { get; set; } = string.Empty;
    public string NombreAlumno { get; set; } = string.Empty;
    public string NombreCurso { get; set; } = string.Empty;

    /// <summary>
    /// ⚠️ NOTA DE MIGRACIÓN: FechaMatricula fue renombrada a FechaRegistro (campo de auditoría)
    /// Para compatibilidad, mapeamos FechaMatricula a FechaRegistro
    /// </summary>
    public DateTime FechaMatricula 
    { 
        get => FechaRegistro ?? DateTime.Now;
        set => FechaRegistro = value;
    }
}
