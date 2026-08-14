using System.Windows;
using System.Windows.Threading;

namespace MarcTime.UI.Vistas;

/// <summary>
/// Cuenta regresiva final antes del cierre. Tiene su PROPIO timer de 1
/// segundo (independiente del sondeo general de limites) para que el
/// conteo visual sea exacto, sin depender de la frecuencia del sondeo.
/// Al llegar a 0, ejecuta alFinalizar (el cierre real de la app) y se cierra.
/// </summary>
public partial class CuentaRegresivaWindow : Window
{
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
    private int _segundosRestantes;
    private readonly Action _alFinalizar;

    public CuentaRegresivaWindow(string nombreApp, int segundosIniciales, Action alFinalizar)
    {
        InitializeComponent();
        TextoApp.Text = $"{nombreApp} alcanzó su límite de tiempo";
        _segundosRestantes = Math.Max(segundosIniciales, 0);
        _alFinalizar = alFinalizar;
        TextoCuenta.Text = _segundosRestantes.ToString();

        _timer.Tick += Timer_Tick;
        _timer.Start();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        _segundosRestantes--;

        if (_segundosRestantes <= 0)
        {
            _timer.Stop();
            TextoCuenta.Text = "0";
            _alFinalizar();
            Close();
            return;
        }

        TextoCuenta.Text = _segundosRestantes.ToString();
    }
}