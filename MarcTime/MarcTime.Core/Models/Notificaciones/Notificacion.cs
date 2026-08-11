namespace MarcTime.Core.Models.Notificaciones;

/// <summary>
/// Refleja Notificaciones: historial de avisos enviados. A lo sumo una de
/// TareaId / HorarioClaseId / AplicacionId puede tener valor (o ninguna,
/// para notificaciones generales del sistema) - mismo CHECK que en la base
/// de datos.
/// </summary>
public class Notificacion
{
    public long NotificacionId { get; set; }
    public int UsuarioId { get; set; }
    public int TipoEventoId { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public DateTime FechaHoraEnvio { get; set; }
    public bool Leida { get; set; }
    public int? TareaId { get; set; }
    public int? HorarioClaseId { get; set; }
    public int? AplicacionId { get; set; }
}