using SIGEC.Certificados.Web.Models.Entities;

namespace SIGEC.Certificados.Web.Models.ViewModels;

public class MatricularViewModel
{
    public IEnumerable<Curso> Cursos { get; set; } = Enumerable.Empty<Curso>();
    public IEnumerable<Usuarios> Alumnos { get; set; } = Enumerable.Empty<Usuarios>();
}
