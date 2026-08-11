namespace MarcTime.Core.Models.Notificaciones;

/// <summary>
/// Refleja Sonidos. UsuarioId nulo significa sonido predeterminado del
/// sistema, disponible para todos.
/// </summary>
public class Sonido
{
    public int SonidoId { get; set; }
    public int? UsuarioId { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
    public bool EsPredeterminado { get; set; }
}