namespace MarcTime.Core.Models.Notificaciones;

/// <summary>
/// Refleja ConfiguracionNotificaciones: que sonido y con cuanta antelacion
/// se avisa cada TipoEvento, por usuario.
/// </summary>
public class ConfiguracionNotificacion
{
    public int ConfiguracionNotificacionId { get; set; }
    public int UsuarioId { get; set; }
    public int TipoEventoId { get; set; }
    public int? SonidoId { get; set; }
    public int MinutosAntelacion { get; set; } = 10;
    public bool Activo { get; set; } = true;
}