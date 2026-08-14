namespace MarcTime.Core.Consultas;

/// <summary>
/// Proyeccion de lectura: cruza el limite configurado en Aplicaciones con lo
/// realmente usado hoy (sesiones cerradas + sesion activa en tiempo real).
/// Trabaja en SEGUNDOS (no minutos) porque la cuenta regresiva final necesita
/// precision de segundo a segundo.
/// </summary>
public class EstadoLimiteAplicacion
{
    public int AplicacionId { get; set; }
    public string NombreEjecutable { get; set; } = string.Empty;
    public string NombreVisible { get; set; } = string.Empty;
    public int? LimiteMinutosDiarios { get; set; }
    public int SegundosUsadosHoy { get; set; }

    public int? LimiteSegundosDiarios =>
        LimiteMinutosDiarios is null ? null : LimiteMinutosDiarios.Value * 60;

    public int? SegundosRestantes =>
        LimiteSegundosDiarios is null ? null : Math.Max(0, LimiteSegundosDiarios.Value - SegundosUsadosHoy);

    public int? MinutosRestantes =>
        SegundosRestantes is null ? null : SegundosRestantes.Value / 60;

    public bool LimiteAlcanzado =>
        LimiteSegundosDiarios is not null && SegundosUsadosHoy >= LimiteSegundosDiarios.Value;
}