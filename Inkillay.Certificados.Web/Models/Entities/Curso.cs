namespace Inkillay.Certificados.Web.Models.Entities;

/// <summary>
/// Entidad Curso según el estándar corporativo actualizado
/// </summary>
public class Curso : AuditoriaBase
{
    public int IdCurso { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Sumilla { get; set; } = string.Empty;
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public int? IdProfesor { get; set; }
    public decimal Costo { get; set; }

    /// <summary>
    /// Estado del curso: 'A' = Activo, 'I' = Inactivo
    /// ⚠️ MIGRACIÓN: Antes era bool, ahora es char(1)
    /// </summary>
    public char Estado { get; set; } = 'A';

    // Propiedad auxiliar para compatibilidad con código legacy
    public bool EstadoActivo => Estado == 'A';
}
