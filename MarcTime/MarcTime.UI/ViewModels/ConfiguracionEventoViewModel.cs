using System.Windows.Input;
using MarcTime.Core.Models.Notificaciones;
using MarcTime.UI.Comun;

namespace MarcTime.UI.ViewModels;

/// <summary>Una fila de configuracion de sonido para un TipoEvento (Clase proxima, Recordatorio de tarea).</summary>
public class ConfiguracionEventoViewModel : ViewModelBase
{
    private Sonido? _sonidoSeleccionado;
    private bool _activo = true;

    public ConfiguracionEventoViewModel(int tipoEventoId, string nombreEvento, List<Sonido> sonidosDisponibles, Action<string?> probarSonido)
    {
        TipoEventoId = tipoEventoId;
        NombreEvento = nombreEvento;
        SonidosDisponibles = sonidosDisponibles;
        ProbarSonidoCommand = new RelayCommand(() => probarSonido(SonidoSeleccionado?.RutaArchivo));
    }

    public int TipoEventoId { get; }
    public string NombreEvento { get; }
    public List<Sonido> SonidosDisponibles { get; }

    public Sonido? SonidoSeleccionado
    {
        get => _sonidoSeleccionado;
        set => SetProperty(ref _sonidoSeleccionado, value);
    }

    public bool Activo
    {
        get => _activo;
        set => SetProperty(ref _activo, value);
    }

    public ICommand ProbarSonidoCommand { get; }
}