namespace MarcTime.Core.Consultas;

/// <summary>
/// Minutos totales usados en una app dentro de un rango de fechas, con su
/// categoria (si tiene). No refleja una tabla 1 a 1.
/// </summary>
public class AppUsoResumen
{
    public int AplicacionId { get; set; }
    public string NombreVisible { get; set; } = string.Empty;
    public string? NombreCategoria { get; set; }
    public bool EsProductiva { get; set; }
    public int MinutosTotales { get; set; }
}