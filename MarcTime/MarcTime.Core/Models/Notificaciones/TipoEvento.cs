namespace MarcTime.Core.Models.Notificaciones;

/// <summary>
/// Refleja TiposEvento: catalogo fijo (ClaseProxima, TareaProxima,
/// LimiteTiempo, MetaSemanalSuperada).
/// </summary>
public class TipoEvento
{
    public int TipoEventoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}