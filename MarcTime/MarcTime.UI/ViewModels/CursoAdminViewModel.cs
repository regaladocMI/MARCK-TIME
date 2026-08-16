using MarcTime.Core.Models.Academico;
using MarcTime.UI.Comun;

namespace MarcTime.UI.ViewModels;

/// <summary>
/// Fila editable de un Curso en la pantalla de administracion. Envuelve el
/// Curso completo (no solo el Id) para no perder PeriodoId/Codigo/Color al
/// guardar - solo se cambia Nombre, el resto queda intacto.
/// </summary>
public class CursoAdminViewModel : ViewModelBase
{
    private string _nombre;

    public CursoAdminViewModel(Curso curso)
    {
        Curso = curso;
        _nombre = curso.Nombre;
    }

    public Curso Curso { get; }
    public int CursoId => Curso.CursoId;

    public string Nombre
    {
        get => _nombre;
        set => SetProperty(ref _nombre, value);
    }
}