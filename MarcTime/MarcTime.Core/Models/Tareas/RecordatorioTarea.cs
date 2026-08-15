namespace MarcTime.Core.Models.Tareas;

/// <summary>
/// Refleja RecordatoriosTarea: un aviso escalonado para una tarea, a X
/// minutos antes de FechaEntrega. Una tarea puede tener varios (3 dias
/// antes, 1 dia antes, 2 horas antes, etc.), cada uno se envia una sola vez.
/// </summary>
public class RecordatorioTarea
{
    public long RecordatorioTareaId { get; set; }
    public int TareaId { get; set; }
    public int MinutosAntelacion { get; set; }
    public bool Enviado { get; set; }
}