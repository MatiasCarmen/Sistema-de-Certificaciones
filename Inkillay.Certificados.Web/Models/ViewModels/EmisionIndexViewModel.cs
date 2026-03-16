using Inkillay.Certificados.Web.Models.Entities;

namespace Inkillay.Certificados.Web.Models.ViewModels;

public class EmisionIndexViewModel
{
    public int? IdCursoSeleccionado { get; set; }
    public IEnumerable<Curso> Cursos { get; set; } = Enumerable.Empty<Curso>();
    public IEnumerable<Modulo> Alumnos { get; set; } = Enumerable.Empty<Modulo>();
    public IEnumerable<Plantilla> Plantillas { get; set; } = Enumerable.Empty<Plantilla>();
}
