using System.Windows.Input;
using MarcTime.UI.Comun;

namespace MarcTime.UI.ViewModels;

/// <summary>
/// ViewModel de la ventana principal: mantiene una instancia de cada
/// modulo y expone comandos de navegacion que cambian cual se muestra.
/// </summary>
public class MainViewModel : ViewModelBase
{
    private object _currentViewModel;

    public MainViewModel(
        InicioViewModel inicio,
        HorarioViewModel horario,
        TareasViewModel tareas,
        HistorialViewModel historial,
        ConfiguracionViewModel configuracion)
    {
        InicioViewModel = inicio;
        HorarioViewModel = horario;
        TareasViewModel = tareas;
        HistorialViewModel = historial;
        ConfiguracionViewModel = configuracion;

        _currentViewModel = inicio;

        NavegarInicioCommand = new RelayCommand(() => CurrentViewModel = InicioViewModel);
        NavegarHorarioCommand = new RelayCommand(() => CurrentViewModel = HorarioViewModel);
        NavegarTareasCommand = new RelayCommand(() => CurrentViewModel = TareasViewModel);
        NavegarHistorialCommand = new RelayCommand(() => CurrentViewModel = HistorialViewModel);
        NavegarConfiguracionCommand = new RelayCommand(() => CurrentViewModel = ConfiguracionViewModel);
    }

    public InicioViewModel InicioViewModel { get; }
    public HorarioViewModel HorarioViewModel { get; }
    public TareasViewModel TareasViewModel { get; }
    public HistorialViewModel HistorialViewModel { get; }
    public ConfiguracionViewModel ConfiguracionViewModel { get; }

    public object CurrentViewModel
    {
        get => _currentViewModel;
        set => SetProperty(ref _currentViewModel, value);
    }

    public ICommand NavegarInicioCommand { get; }
    public ICommand NavegarHorarioCommand { get; }
    public ICommand NavegarTareasCommand { get; }
    public ICommand NavegarHistorialCommand { get; }
    public ICommand NavegarConfiguracionCommand { get; }
}