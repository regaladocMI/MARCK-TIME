using System.Windows.Input;

namespace MarcTime.UI.Comun;

/// <summary>Implementacion generica de ICommand para conectar botones del XAML a metodos de C#.</summary>
public class RelayCommand : ICommand
{
    private readonly Action _ejecutar;
    private readonly Func<bool>? _puedeEjecutar;

    public RelayCommand(Action ejecutar, Func<bool>? puedeEjecutar = null)
    {
        _ejecutar = ejecutar;
        _puedeEjecutar = puedeEjecutar;
    }

    public bool CanExecute(object? parametro) => _puedeEjecutar?.Invoke() ?? true;
    public void Execute(object? parametro) => _ejecutar();

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}