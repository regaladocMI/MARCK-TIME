using MarcTime.Core.Models.Monitoreo;
using MarcTime.UI.Comun;

namespace MarcTime.UI.ViewModels;

/// <summary>Envuelve una Aplicacion con un checkbox, para la lista de "restringir a estas apps" del dialogo de bloque.</summary>
public class AppSeleccionableViewModel : ViewModelBase
{
    private bool _seleccionada;

    public AppSeleccionableViewModel(Aplicacion aplicacion)
    {
        Aplicacion = aplicacion;
    }

    public Aplicacion Aplicacion { get; }
    public string NombreVisible => Aplicacion.NombreVisible;

    public bool Seleccionada
    {
        get => _seleccionada;
        set => SetProperty(ref _seleccionada, value);
    }
}