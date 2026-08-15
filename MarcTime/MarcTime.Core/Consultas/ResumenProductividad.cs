namespace MarcTime.Core.Consultas;

/// <summary>
/// Distribucion de minutos segun productividad de la categoria de cada app,
/// dentro de un rango de fechas.
/// </summary>
public class ResumenProductividad
{
    public int MinutosProductivos { get; set; }
    public int MinutosNoProductivos { get; set; }
    public int MinutosSinCategoria { get; set; }

    public int MinutosTotales => MinutosProductivos + MinutosNoProductivos + MinutosSinCategoria;

    public double PorcentajeProductivo =>
        MinutosTotales == 0 ? 0 : Math.Round(MinutosProductivos * 100.0 / MinutosTotales, 1);
}