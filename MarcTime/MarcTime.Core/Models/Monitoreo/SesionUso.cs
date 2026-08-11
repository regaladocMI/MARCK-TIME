namespace MarcTime.Core.Models.Monitoreo;

/// <summary>
/// Refleja SesionesUso: registro crudo de cada vez que una app pasa a primer
/// plano y deja de estarlo. DuracionSegundos la calcula SQL Server (columna
/// persistida); aqui solo se lee, nunca se asigna manualmente.
/// </summary>
public class SesionUso
{
    public long SesionUsoId { get; set; }
    public int AplicacionId { get; set; }
    public DateTime FechaHoraInicio { get; set; }
    public DateTime? FechaHoraFin { get; set; }
    public int? DuracionSegundos { get; set; }
    public string? TituloVentana { get; set; }
}