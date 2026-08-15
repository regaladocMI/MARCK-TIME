namespace MarcTime.Core.Consultas;

/// <summary>
/// App que esta vigilada por horario (tiene al menos una fila en
/// RestriccionesHorarioApp) y ahora mismo no pertenece a ningun bloque
/// permitido para el dia y hora actuales. No refleja una tabla 1 a 1.
/// </summary>
public class AppFueraDeHorario
{
    public int AplicacionId { get; set; }
    public string NombreEjecutable { get; set; } = string.Empty;
    public string NombreVisible { get; set; } = string.Empty;
}