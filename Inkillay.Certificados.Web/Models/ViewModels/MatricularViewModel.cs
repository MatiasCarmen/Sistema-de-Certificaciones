using Inkillay.Certificados.Web.Models.Entities;

namespace Inkillay.Certificados.Web.Models.ViewModels;

public class MatricularViewModel
{
    public IEnumerable<Curso> Cursos { get; set; } = Enumerable.Empty<Curso>();
    public IEnumerable<UsuarioViewModel> Alumnos { get; set; } = Enumerable.Empty<UsuarioViewModel>();
}
