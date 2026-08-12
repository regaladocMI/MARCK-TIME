namespace MarcTime.Core.Consultas;

/// <summary>
/// Proyeccion de lectura (no refleja una tabla 1 a 1): cruza el limite
/// configurado en Aplicaciones con lo realmente usado hoy segun
/// ResumenUsoDiario. La usa la UI para mostrar progreso, y la Seccion 8
/// para decidir cuando avisar/cerrar.
/// </summary>
public class EstadoLimiteAplicacion
{
    public int AplicacionId { get; set; }
    public string NombreVisible { get; set; } = string.Empty;
    public int? LimiteMinutosDiarios { get; set; }
    public int MinutosUsadosHoy { get; set; }

    /// <summary>Null si la app no tiene limite configurado (sin tope).</summary>
    public int? MinutosRestantes =>
        LimiteMinutosDiarios is null ? null : Math.Max(0, LimiteMinutosDiarios.Value - MinutosUsadosHoy);

    public bool LimiteAlcanzado =>
        LimiteMinutosDiarios is not null && MinutosUsadosHoy >= LimiteMinutosDiarios.Value;
}