namespace Inkillay.Certificados.Web.Models.Entities;
public class Modulo : AuditoriaBase
{
    // 1. Identificadores Principales
    public int IdModulo { get; set; }
    public int IdCurso { get; set; }
    public int IdDocente { get; set; }

    // 2. Información del Módulo (Lo que ves en la pantalla azul)
    public string EdicionCurso { get; set; } = string.Empty;
    public decimal CostoCurso { get; set; }
    public int CapacidadAlumno { get; set; }
    public char Modalidad { get; set; } = 'P'; // 'P'=Presencial, 'R'=Remoto, 'H'=Híbrido
    public string Sumilla { get; set; } = string.Empty;

    // 3. Cronograma 
    public DateTime? FechaInicioMatricula { get; set; }
    public DateTime? FechaFinMatricula { get; set; }
    public DateTime? FechaInicioClases { get; set; }

    // 4. Estado (Ajustado al nombre real de tu tabla: EstadoMatricula)
    // Valores: 1-Creado, 2-En Matricula, etc.
    public char EstadoMatricula { get; set; } = '1';

    // 5. Propiedades de Apoyo (Para mostrar nombres en la lista, no    // Propiedades de apoyo (JOINs en SP)
    public string NombreCurso { get; set; } = string.Empty;
    public string NombreDocente { get; set; } = string.Empty;
    public int TotalInscritos { get; set; } // Nuevo campo que debería devolver USP_Modulos_ListarTodos

    // --- Mantenemos lo necesario para no romper el resto del sistema ---
    public char Estado { get => EstadoMatricula; set => EstadoMatricula = value; }
}