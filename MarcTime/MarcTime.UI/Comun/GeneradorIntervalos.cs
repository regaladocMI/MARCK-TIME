namespace MarcTime.UI.Comun;

/// <summary>
/// Genera las franjas horarias del "selector de intervalo". Calcula en
/// MINUTOS TOTALES (no encadenando sumas sobre TimeOnly): TimeOnly.Add() da
/// la vuelta al pasar medianoche (23:45 + 15min = 00:00), lo que en un bucle
/// "while actual menor o igual a fin" nunca termina si fin ya es tarde en el
/// dia - queda dando vueltas para siempre y congela el hilo de la UI.
/// </summary>
public static class GeneradorIntervalos
{
    public static List<OpcionHora> Generar(TimeOnly inicio, TimeOnly fin, TimeSpan paso)
    {
        var opciones = new List<OpcionHora>();

        int totalMinutos = (int)(fin.ToTimeSpan() - inicio.ToTimeSpan()).TotalMinutes;
        int pasoMinutos = (int)paso.TotalMinutes;

        for (int minuto = 0; minuto <= totalMinutos; minuto += pasoMinutos)
        {
            opciones.Add(new OpcionHora(inicio.Add(TimeSpan.FromMinutes(minuto))));
        }

        return opciones;
    }
}

/// <summary>Envuelve un TimeOnly para mostrarlo formateado en un ComboBox (ComboBox usa ToString() por defecto).</summary>
public class OpcionHora
{
    public TimeOnly Valor { get; }
    public OpcionHora(TimeOnly valor) => Valor = valor;
    public override string ToString() => Valor.ToString("HH\\:mm");
}