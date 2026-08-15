namespace MarcTime.Core.Consultas;

/// <summary>Minutos totales usados (todas las apps) en un dia especifico, para graficar tendencia.</summary>
public class UsoPorDia
{
    public DateOnly Fecha { get; set; }
    public int MinutosTotales { get; set; }
}