using System.Windows;
using System.Windows.Controls;
using MarcTime.UI.ViewModels;

namespace MarcTime.UI.Vistas;

public partial class HistorialView : UserControl
{
    public HistorialView()
    {
        InitializeComponent();
    }

    private void BotonBorrarDia_Click(object sender, RoutedEventArgs e)
    {
        if (((Button)sender).Tag is not DateOnly fecha || DataContext is not HistorialViewModel viewModel)
        {
            return;
        }

        var resultado = MessageBox.Show(
            $"¿Borrar todo el historial de uso del {fecha:dd/MM/yyyy}? Esta acción no se puede deshacer.",
            "Confirmar borrado",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (resultado == MessageBoxResult.Yes)
        {
            viewModel.BorrarDia(fecha);
        }
    }
}