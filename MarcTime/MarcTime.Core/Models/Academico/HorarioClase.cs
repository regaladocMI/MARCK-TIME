using MarcTime.Core.Models.Enums;

namespace MarcTime.Core.Models.Academico;

/// <summary>
/// Refleja HorariosClase. Un Curso puede tener varios horarios (ej. lunes y
/// miercoles a distintas horas).
/// </summary>
public class HorarioClase
{
    public int HorarioClaseId { get; set; }
    public int CursoId { get; set; }
    public DiaSemana DiaSemana { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public string? Ubicacion { get; set; }
}