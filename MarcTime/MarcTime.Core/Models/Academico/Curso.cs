namespace MarcTime.Core.Models.Academico;

/// <summary>
/// Refleja Cursos. A pesar del nombre, representa cualquier bloque
/// recurrente con horario (materia, catedra, cliente, turno de trabajo).
/// PeriodoId es opcional.
/// </summary>
public class Curso
{
    public int CursoId { get; set; }
    public int UsuarioId { get; set; }
    public int? PeriodoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public string? Color { get; set; }
}