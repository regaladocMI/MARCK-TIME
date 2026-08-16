using System.Collections.ObjectModel;
using System.Windows.Input;
using MarcTime.Core.Consultas;
using MarcTime.Data.Repositories;
using MarcTime.UI.Comun;

namespace MarcTime.UI.ViewModels;

/// <summary>
/// Pantalla de historial: grafica de barras de los ultimos 7 dias (total de
/// todas las apps por defecto, o de UNA app especifica si el usuario hace
/// clic en ella en la lista de abajo) + borrado por dia con confirmacion.
/// </summary>
public class HistorialViewModel : ViewModelBase
{
    private readonly IReporteUsoRepository _reporteRepository;
    private readonly int _usuarioId;
    private int? _aplicacionSeleccionadaId;
    private string _tituloGrafica = "Todas las apps";

    public HistorialViewModel(IReporteUsoRepository reporteRepository, int usuarioId)
    {
        _reporteRepository = reporteRepository;
        _usuarioId = usuarioId;
        RefrescarCommand = new RelayCommand(Cargar);
        SeleccionarAppCommand = new RelayCommand<int>(SeleccionarApp);
        VerTodoCommand = new RelayCommand(VerTodo);
        Cargar();
    }

    public ObservableCollection<DiaUsoBarraViewModel> Barras { get; } = new();
    public ObservableCollection<AppUsoResumen> UsoPorApp { get; } = new();

    public string TituloGrafica
    {
        get => _tituloGrafica;
        set => SetProperty(ref _tituloGrafica, value);
    }

    public bool HayAppSeleccionada => _aplicacionSeleccionadaId is not null;

    public ICommand RefrescarCommand { get; }
    public ICommand SeleccionarAppCommand { get; }
    public ICommand VerTodoCommand { get; }

    private void Cargar()
    {
        DateOnly hoy = DateOnly.FromDateTime(DateTime.Now);
        DateOnly hace6Dias = hoy.AddDays(-6);

        Dictionary<DateOnly, int> porDia = _aplicacionSeleccionadaId is null
            ? _reporteRepository.ObtenerUsoPorDia(_usuarioId, hace6Dias, hoy).ToDictionary(d => d.Fecha, d => d.MinutosTotales)
            : _reporteRepository.ObtenerUsoPorDiaDeApp(_usuarioId, _aplicacionSeleccionadaId.Value, hace6Dias, hoy).ToDictionary(d => d.Fecha, d => d.MinutosTotales);

        var minutosPorDiaCompleto = Enumerable.Range(0, 7)
            .Select(offset => hace6Dias.AddDays(offset))
            .Select(fecha => (Fecha: fecha, Minutos: porDia.GetValueOrDefault(fecha, 0)))
            .ToList();

        int maximo = minutosPorDiaCompleto.Count == 0 ? 0 : minutosPorDiaCompleto.Max(d => d.Minutos);

        Barras.Clear();
        foreach (var (fecha, minutos) in minutosPorDiaCompleto)
        {
            Barras.Add(new DiaUsoBarraViewModel(fecha, minutos, maximo));
        }

        UsoPorApp.Clear();
        foreach (var app in _reporteRepository.ObtenerUsoPorApp(_usuarioId, hace6Dias, hoy))
        {
            UsoPorApp.Add(app);
        }
    }

    private void SeleccionarApp(int aplicacionId)
    {
        _aplicacionSeleccionadaId = aplicacionId;
        var app = UsoPorApp.FirstOrDefault(a => a.AplicacionId == aplicacionId);
        TituloGrafica = app is null ? "App seleccionada" : app.NombreVisible;
        OnPropertyChanged(nameof(HayAppSeleccionada));
        Cargar();
    }

    private void VerTodo()
    {
        _aplicacionSeleccionadaId = null;
        TituloGrafica = "Todas las apps";
        OnPropertyChanged(nameof(HayAppSeleccionada));
        Cargar();
    }

    /// <summary>Llamado desde la Vista, DESPUES de que el usuario confirmo el borrado en un dialogo.</summary>
    public void BorrarDia(DateOnly fecha)
    {
        _reporteRepository.BorrarHistorialDia(_usuarioId, fecha);
        Cargar();
    }
}