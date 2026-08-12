using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using MarcTime.Core.Deteccion;

namespace MarcTime.UI;

public partial class App : Application
{
    private readonly IDetectorAppActiva _detector = new DetectorAppActiva();
    private DispatcherTimer? _timerPrueba;
    private int _muestrasRestantes = 5;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Debug.WriteLine("=== Prueba Seccion 5: cambia de ventana (Alt+Tab) durante los proximos 10 segundos ===");

        _timerPrueba = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _timerPrueba.Tick += (_, _) =>
        {
            AppActivaInfo? appActiva = _detector.ObtenerAppActiva();

            Debug.WriteLine(appActiva is null
                ? "No se pudo detectar la app activa en este instante."
                : $"App activa -> Ejecutable: {appActiva.NombreEjecutable} | Titulo: \"{appActiva.TituloVentana}\"");

            _muestrasRestantes--;
            if (_muestrasRestantes <= 0)
            {
                _timerPrueba!.Stop();
                Debug.WriteLine("=== Prueba Seccion 5 finalizada ===");
            }
        };
        _timerPrueba.Start();
    }
}