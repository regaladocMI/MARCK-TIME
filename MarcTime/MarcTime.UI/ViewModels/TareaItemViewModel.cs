using MarcTime.Core.Models.Enums;
using MarcTime.Core.Models.Tareas;
using MarcTime.UI.Comun;

namespace MarcTime.UI.ViewModels;

/// <summary>Envuelve una Tarea para mostrarla en la lista, con el checkbox "completada" persistiendo al instante.</summary>
public class TareaItemViewModel : ViewModelBase
{
    private bool _completada;

    public TareaItemViewModel(Tarea tarea)
    {
        TareaId = tarea.TareaId;
        Titulo = tarea.Titulo;
        FechaEntrega = tarea.FechaEntrega;
        Prioridad = tarea.Prioridad;
        _completada = tarea.Completada;
    }

    public int TareaId { get; }
    public string Titulo { get; }
    public DateTime FechaEntrega { get; }
    public PrioridadTarea Prioridad { get; }

    public string FechaEntregaTexto => FechaEntrega.ToString("dd/MM/yyyy HH:mm");

    public string PrioridadTexto => Prioridad switch
    {
        PrioridadTarea.Alta => "Alta",
        PrioridadTarea.Media => "Media",
        _ => "Baja"
    };

    public bool Completada
    {
        get => _completada;
        set
        {
            if (SetProperty(ref _completada, value))
            {
                CompletadaCambiada?.Invoke(this, value);
            }
        }
    }

    /// <summary>Se dispara cuando el usuario marca/desmarca el checkbox - TareasViewModel lo escucha para persistir.</summary>
    public event Action<TareaItemViewModel, bool>? CompletadaCambiada;
}