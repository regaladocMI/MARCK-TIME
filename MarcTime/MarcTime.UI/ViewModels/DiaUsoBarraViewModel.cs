namespace MarcTime.UI.ViewModels;

/// <summary>Una columna de la grafica de barras del historial: dia + minutos + altura ya calculada en pixeles.</summary>
public class DiaUsoBarraViewModel
{
    private const double AlturaMaximaPx = 140;

    public DiaUsoBarraViewModel(DateOnly fecha, int minutosTotales, int minutosMaximoDeLaSemana)
    {
        Fecha = fecha;
        MinutosTotales = minutosTotales;
        EtiquetaDia = fecha.ToString("ddd dd").Replace(".", "");
        AlturaBarra = minutosMaximoDeLaSemana == 0
            ? 4
            : Math.Max(4, minutosTotales * AlturaMaximaPx / minutosMaximoDeLaSemana);
    }

    public DateOnly Fecha { get; }
    public int MinutosTotales { get; }
    public string EtiquetaDia { get; }
    public double AlturaBarra { get; }
}