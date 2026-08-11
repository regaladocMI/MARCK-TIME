namespace MarcTime.Core.Models.Monitoreo;

/// <summary>
/// Refleja ResumenUsoDiario: minutos totales por app y por dia. Se mantiene
/// sincronizada automaticamente por un trigger en SQL Server, no se escribe
/// manualmente desde C#.
/// </summary>
public class ResumenUsoDiario
{
    public long ResumenUsoDiarioId { get; set; }
    public int AplicacionId { get; set; }
    public DateOnly Fecha { get; set; }
    public int MinutosTotales { get; set; }
}