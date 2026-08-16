using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MarcTime.UI.Comun;

/// <summary>
/// Base para todos los ViewModels: implementa INotifyPropertyChanged para
/// que WPF refresque la pantalla automaticamente cuando una propiedad cambia.
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string nombrePropiedad) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombrePropiedad));

    protected bool SetProperty<T>(ref T campo, T valor, [CallerMemberName] string? nombrePropiedad = null)
    {
        if (Equals(campo, valor)) return false;
        campo = valor;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombrePropiedad));
        return true;
    }
}