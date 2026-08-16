namespace MarcTime.UI.ViewModels;

/// <summary>Envuelve un Curso (o "Sin curso") para el ComboBox del formulario de tarea.</summary>
public class CursoOpcionViewModel
{
    public CursoOpcionViewModel(int? cursoId, string nombre)
    {
        CursoId = cursoId;
        Nombre = nombre;
    }

    public int? CursoId { get; }
    public string Nombre { get; }
    public override string ToString() => Nombre;
}