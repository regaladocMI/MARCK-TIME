using MarcTime.Core.Models.Enums;

namespace MarcTime.Core.Consultas;

/// <summary>
/// HorarioClase + datos de su Curso ya resueltos (nombre, color), listo
/// para pintar en la pantalla sin hacer una consulta aparte por cada tarjeta.
/// </summary>
public class BloqueHorarioDetalle
{
    public int HorarioClaseId { get; set; }
    public int CursoId { get; set; }
    public string NombreCurso { get; set; } = string.Empty;
    public string? Color { get; set; }
    public DiaSemana DiaSemana { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public string? Ubicacion { get; set; }
}