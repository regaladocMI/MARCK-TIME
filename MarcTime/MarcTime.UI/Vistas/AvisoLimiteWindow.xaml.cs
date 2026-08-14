using System.Windows;
using System.Windows.Threading;

namespace MarcTime.UI.Vistas;

/// <summary>
/// Aviso temprano, NO MODAL a proposito: usar ShowDialog() (modal) causaba
/// que el timer de limites se reactivara "por dentro" mientras el aviso
/// seguia abierto (WPF bombea mensajes durante un dialogo modal), lo que
/// producia cierres de app fuera de orden. Con Show() esto no pasa.
/// Se autocierra sola a los 8 segundos si el usuario no la cierra antes.
/// </summary>
public partial class AvisoLimiteWindow : Window
{
    private readonly DispatcherTimer _autocierre = new() { Interval = TimeSpan.FromSeconds(8) };

    public AvisoLimiteWindow(string mensaje)
    {
        InitializeComponent();
        TextoMensaje.Text = mensaje;

        _autocierre.Tick += (_, _) =>
        {
            _autocierre.Stop();
            Close();
        };
        _autocierre.Start();
    }

    private void BotonEntendido_Click(object sender, RoutedEventArgs e) => Close();
}