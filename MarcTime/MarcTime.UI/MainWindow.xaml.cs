using System.ComponentModel;
using System.Windows;

namespace MarcTime.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        // La app vive en la bandeja del sistema (Seccion 11): cerrar con la X
        // no debe terminar el proceso, solo esconder la ventana. Se reabre
        // con doble clic en el icono de bandeja (ver ServicioNotificaciones).
        e.Cancel = true;
        Hide();
    }
}