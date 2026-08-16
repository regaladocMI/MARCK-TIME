using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MarcTime.UI.Comun;

/// <summary>Convierte bool a Visibility para bindings de Visibility en XAML (ej. mostrar el boton "Ver todo" solo si hay una app seleccionada).</summary>
public class BoolAVisibilidadConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parametro, CultureInfo culture) =>
        value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object? value, Type targetType, object? parametro, CultureInfo culture) =>
        throw new NotSupportedException();
}