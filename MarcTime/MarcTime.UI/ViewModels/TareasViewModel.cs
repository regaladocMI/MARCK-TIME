using System.Collections.ObjectModel;
using System.Windows.Input;
using MarcTime.Data.Repositories;
using MarcTime.UI.Comun;
using MarcTime.UI.Vistas;

namespace MarcTime.UI.ViewModels;

/// <summary>Pantalla de agenda de tareas: lista + crear/editar/eliminar + toggle de completada.</summary>
public class TareasViewModel : ViewModelBase
{
    private readonly ITareaRepository _tareaRepository;
    private readonly ICursoRepository _cursoRepository;
    private readonly IRecordatorioTareaRepository _recordatorioRepository;
    private readonly int _usuarioId;

    public TareasViewModel(
        ITareaRepository tareaRepository,
        ICursoRepository cursoRepository,
        IRecordatorioTareaRepository recordatorioRepository,
        int usuarioId)
    {
        _tareaRepository = tareaRepository;
        _cursoRepository = cursoRepository;
        _recordatorioRepository = recordatorioRepository;
        _usuarioId = usuarioId;

        AgregarTareaCommand = new RelayCommand(AgregarTarea);
        EditarTareaCommand = new RelayCommand<int>(EditarTarea);
        EliminarTareaCommand = new RelayCommand<int>(EliminarTarea);

        Cargar();
    }

    public ObservableCollection<TareaItemViewModel> Tareas { get; } = new();

    public ICommand AgregarTareaCommand { get; }
    public ICommand EditarTareaCommand { get; }
    public ICommand EliminarTareaCommand { get; }

    private void Cargar()
    {
        foreach (var item in Tareas)
        {
            item.CompletadaCambiada -= Item_CompletadaCambiada;
        }
        Tareas.Clear();

        foreach (var tarea in _tareaRepository.ObtenerTodas(_usuarioId))
        {
            var item = new TareaItemViewModel(tarea);
            item.CompletadaCambiada += Item_CompletadaCambiada;
            Tareas.Add(item);
        }
    }

    private void Item_CompletadaCambiada(TareaItemViewModel item, bool nuevoValor)
    {
        _tareaRepository.MarcarCompletada(item.TareaId, nuevoValor);
    }

    private void AgregarTarea()
    {
        var ventana = new EditarTareaWindow(_cursoRepository, _tareaRepository, _recordatorioRepository, _usuarioId);
        if (ventana.ShowDialog() == true)
        {
            Cargar();
        }
    }

    private void EditarTarea(int tareaId)
    {
        var tarea = _tareaRepository.ObtenerPorId(tareaId);
        if (tarea is null) return;

        var ventana = new EditarTareaWindow(_cursoRepository, _tareaRepository, _recordatorioRepository, _usuarioId, tareaExistente: tarea);
        if (ventana.ShowDialog() == true)
        {
            Cargar();
        }
    }

    private void EliminarTarea(int tareaId)
    {
        _tareaRepository.Eliminar(tareaId); // RecordatoriosTarea se borran solos (CASCADE, Seccion 14)
        Cargar();
    }
}