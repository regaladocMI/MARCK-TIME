using MarcTime.Core.Models.Enums;

namespace MarcTime.Core.Models.Tareas;

/// <summary>
/// Refleja Tareas. RowVersion habilita concurrencia optimista: la capa de
/// datos compara este valor antes de un UPDATE para detectar ediciones
/// simultaneas.
/// </summary>
public class Tarea
{
    public int TareaId { get; set; }
    public int UsuarioId { get; set; }
    public int? CursoId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public DateTime FechaEntrega { get; set; }
    public PrioridadTarea Prioridad { get; set; } = PrioridadTarea.Media;
    public bool Completada { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public byte[]? RowVersion { get; set; }
}