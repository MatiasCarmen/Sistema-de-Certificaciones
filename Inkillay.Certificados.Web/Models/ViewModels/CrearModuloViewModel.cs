using Inkillay.Certificados.Web.Models.Entities;

namespace Inkillay.Certificados.Web.Models.ViewModels;

public class CrearModuloViewModel
{
    public IEnumerable<Curso> Cursos { get; set; } = Enumerable.Empty<Curso>();
    public IEnumerable<Inkillay.Certificados.Web.Models.Entities.Usuarios> Docentes { get; set; } = Enumerable.Empty<Inkillay.Certificados.Web.Models.Entities.Usuarios>();
    
    // Propiedades para el POST
    public int IdCurso { get; set; }
    public int IdDocente { get; set; }
    public string EdicionCurso { get; set; } = string.Empty;
    public decimal CostoCurso { get; set; }
    public int CapacidadAlumno { get; set; }
    public DateTime FechaInicioMatricula { get; set; } = DateTime.Now;
    public DateTime FechaFinMatricula { get; set; } = DateTime.Now.AddDays(15);
    public DateTime FechaInicioClases { get; set; } = DateTime.Now.AddDays(20);
    public char Modalidad { get; set; } = 'P';
    public char EstadoMatricula { get; set; } = '1';
    public string Sumilla { get; set; } = string.Empty;
}
