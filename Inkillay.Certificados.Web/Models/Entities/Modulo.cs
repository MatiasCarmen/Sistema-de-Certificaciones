namespace Inkillay.Certificados.Web.Models.Entities;

/// <summary>
/// Entidad Matrícula según el estándar corporativo actualizado
/// </summary>
public class Modulo : AuditoriaBase
{
    public int IdModulo { get; set; }
    public int IdCurso { get; set; }
    public int IdDocente { get; set; }

    public string EdicionCurso { get; set; } = string.Empty;

    public DateTime? FechaInicioMatricula { get; set; }
    public DateTime? FechaFinMatricula { get; set; }
    public DateTime? FechaInicioClases { get; set; }

    public decimal CostoCurso { get; set; }
    public int CapacidadAlumno { get; set; }

    /// <summary>
    /// Modalidad del cursado: 'P'=Presencial, 'R'=Remoto, 'H'=Híbrido
    /// </summary>
    public char Modalidad { get; set; } = 'P';

    /// <summary>
    /// Estado del registro (A=Activo, I=Inactivo)
    /// </summary>
    public char Estado { get; set; } = 'A';

    // Propiedades calculadas (opcionales para reportes)
    public string NombreCurso { get; set; } = string.Empty;
    public string NombreDocente { get; set; } = string.Empty;
    public int TotalInscritos { get; set; }

    // Propiedades Legacy / Calculadas (para no romper otras vistas/controladores durante la migración)
    public bool Aprobado { get; set; }
    public decimal TotalPagado { get; set; }
    public string Sumilla { get; set; } = string.Empty;
    public string NombreAlumno { get; set; } = string.Empty;
    
    public DateTime FechaModulo 
    { 
        get => FechaRegistro ?? DateTime.Now;
        set => FechaRegistro = value;
    }
    
    // Suplanta al IdAlumno para vistas legacy
    public int IdUsuario { get; set; }
}
