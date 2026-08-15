using System.Windows.Input;

namespace MarcTime.UI.Comun;

/// <summary>Version de RelayCommand que recibe un parametro (ej. el dia de la semana del boton "+ Agregar").</summary>
public class RelayCommand<T> : ICommand
{
    private readonly Action<T?> _ejecutar;
    private readonly Func<T?, bool>? _puedeEjecutar;

    public RelayCommand(Action<T?> ejecutar, Func<T?, bool>? puedeEjecutar = null)
    {
        _ejecutar = ejecutar;
        _puedeEjecutar = puedeEjecutar;
    }

    public bool CanExecute(object? parametro) => _puedeEjecutar?.Invoke((T?)parametro) ?? true;
    public void Execute(object? parametro) => _ejecutar((T?)parametro);

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}