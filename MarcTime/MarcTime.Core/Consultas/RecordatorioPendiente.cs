namespace MarcTime.Core.Consultas;

/// <summary>Un recordatorio pendiente de enviar, con los datos de su tarea ya incluidos.</summary>
public class RecordatorioPendiente
{
    public long RecordatorioTareaId { get; set; }
    public int TareaId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public DateTime FechaEntrega { get; set; }
    public int MinutosAntelacion { get; set; }
}