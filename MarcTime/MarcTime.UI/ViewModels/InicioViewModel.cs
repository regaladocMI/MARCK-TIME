using System.Collections.ObjectModel;
using System.Windows.Input;
using MarcTime.Core.Consultas;
using MarcTime.Data.Repositories;
using MarcTime.UI.Comun;

namespace MarcTime.UI.ViewModels;

/// <summary>
/// Pantalla de inicio: resumen del uso de HOY, reutilizando
/// ReporteUsoRepository (Seccion 13) - misma consulta, ahora con destino visual.
/// </summary>
public class InicioViewModel : ViewModelBase
{
    private readonly IReporteUsoRepository _reporteRepository;
    private readonly int _usuarioId;
    private ResumenProductividad _productividad = new();

    public InicioViewModel(IReporteUsoRepository reporteRepository, int usuarioId)
    {
        _reporteRepository = reporteRepository;
        _usuarioId = usuarioId;
        RefrescarCommand = new RelayCommand(Cargar);
        Cargar();
    }

    public ObservableCollection<AppUsoResumen> UsoDeHoy { get; } = new();

    public ResumenProductividad Productividad
    {
        get => _productividad;
        set => SetProperty(ref _productividad, value);
    }

    public ICommand RefrescarCommand { get; }

    private void Cargar()
    {
        DateOnly hoy = DateOnly.FromDateTime(DateTime.Now);

        UsoDeHoy.Clear();
        foreach (var app in _reporteRepository.ObtenerUsoPorApp(_usuarioId, hoy, hoy))
        {
            UsoDeHoy.Add(app);
        }

        Productividad = _reporteRepository.ObtenerResumenProductividad(_usuarioId, hoy, hoy);
    }
}