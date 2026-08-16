using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using MarcTime.Core.Models.Enums;
using MarcTime.Core.Models.Tareas;
using MarcTime.Data.Repositories;
using MarcTime.UI.ViewModels;

namespace MarcTime.UI.Vistas;

/// <summary>
/// Formulario de tarea, sirve para crear y editar (mismo patron que
/// EditarBloqueWindow de la Seccion 16). Gestiona la lista de recordatorios
/// como una coleccion editable en memoria; al guardar, compara contra la
/// lista original para saber que Crear() y que Eliminar().
/// </summary>
public partial class EditarTareaWindow : Window
{
    private readonly ICursoRepository _cursoRepository;
    private readonly ITareaRepository _tareaRepository;
    private readonly IRecordatorioTareaRepository _recordatorioRepository;
    private readonly int _usuarioId;
    private readonly Tarea? _tareaExistente;
    private readonly ObservableCollection<RecordatorioEditableViewModel> _recordatorios = new();
    private readonly HashSet<long> _recordatorioIdsOriginales = new();

    public EditarTareaWindow(
        ICursoRepository cursoRepository,
        ITareaRepository tareaRepository,
        IRecordatorioTareaRepository recordatorioRepository,
        int usuarioId,
        Tarea? tareaExistente = null)
    {
        InitializeComponent();

        _cursoRepository = cursoRepository;
        _tareaRepository = tareaRepository;
        _recordatorioRepository = recordatorioRepository;
        _usuarioId = usuarioId;
        _tareaExistente = tareaExistente;

        ListaRecordatorios.ItemsSource = _recordatorios;

        var opcionesCurso = new List<CursoOpcionViewModel> { new(null, "(Sin curso)") };
        opcionesCurso.AddRange(_cursoRepository.ObtenerTodos(usuarioId).Select(c => new CursoOpcionViewModel(c.CursoId, c.Nombre)));
        ComboCurso.ItemsSource = opcionesCurso;
        ComboCurso.SelectedIndex = 0;

        if (tareaExistente is null)
        {
            TextoTitulo.Text = "Nueva tarea";
            foreach (int offset in new[] { 4320, 1440, 120 }) // 3 dias, 1 dia, 2 horas: mismos defaults de CrearPredeterminados
            {
                _recordatorios.Add(new RecordatorioEditableViewModel(offset));
            }
        }
        else
        {
            TextoTitulo.Text = "Editar tarea";
            PrecargarDatos(tareaExistente, opcionesCurso);
        }
    }

    private void PrecargarDatos(Tarea tarea, List<CursoOpcionViewModel> opcionesCurso)
    {
        TextoTituloTarea.Text = tarea.Titulo;
        TextoDescripcion.Text = tarea.Descripcion ?? string.Empty;

        ComboCurso.SelectedItem = opcionesCurso.FirstOrDefault(o => o.CursoId == tarea.CursoId) ?? opcionesCurso[0];

        TextoFecha.Text = tarea.FechaEntrega.ToString("dd/MM/yyyy");
        TextoHora.Text = tarea.FechaEntrega.ToString("HH\\:mm");
        PlaceholderFecha.Visibility = Visibility.Collapsed;
        PlaceholderHora.Visibility = Visibility.Collapsed;

        ComboPrioridad.SelectedIndex = tarea.Prioridad switch
        {
            PrioridadTarea.Baja => 0,
            PrioridadTarea.Alta => 2,
            _ => 1
        };

        foreach (var recordatorio in _recordatorioRepository.ObtenerPorTarea(tarea.TareaId))
        {
            _recordatorios.Add(new RecordatorioEditableViewModel(recordatorio.MinutosAntelacion, recordatorio.RecordatorioTareaId));
            _recordatorioIdsOriginales.Add(recordatorio.RecordatorioTareaId);
        }
    }

    private void Campo_GotFocus(object sender, RoutedEventArgs e)
    {
        ObtenerPlaceholderDe((TextBox)sender).Visibility = Visibility.Collapsed;
    }

    private void Campo_LostFocus(object sender, RoutedEventArgs e)
    {
        var caja = (TextBox)sender;
        ObtenerPlaceholderDe(caja).Visibility = string.IsNullOrWhiteSpace(caja.Text) ? Visibility.Visible : Visibility.Collapsed;
    }

    private TextBlock ObtenerPlaceholderDe(TextBox caja)
    {
        if (caja == TextoFecha) return PlaceholderFecha;
        if (caja == TextoHora) return PlaceholderHora;
        return PlaceholderValorRecordatorio;
    }
    private void BotonAgregarRecordatorio_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(TextoValorRecordatorio.Text, out int valor) || valor <= 0)
        {
            MostrarError("Ingresa un número válido mayor a 0 para el recordatorio.");
            return;
        }

        int factor = ComboUnidadRecordatorio.SelectedIndex switch
        {
            1 => 60,   // Horas
            2 => 1440, // Dias
            _ => 1     // Minutos
        };
        int minutos = valor * factor;

        if (_recordatorios.Any(r => r.MinutosAntelacion == minutos))
        {
            MostrarError("Ya existe un recordatorio con esa antelación.");
            return;
        }

        _recordatorios.Add(new RecordatorioEditableViewModel(minutos));
        TextoError.Visibility = Visibility.Collapsed;
    }

    private void BotonQuitarRecordatorio_Click(object sender, RoutedEventArgs e)
    {
        if (((Button)sender).Tag is RecordatorioEditableViewModel recordatorio)
        {
            _recordatorios.Remove(recordatorio);
        }
    }

    private void BotonGuardar_Click(object sender, RoutedEventArgs e)
    {
        TextoError.Visibility = Visibility.Collapsed;

        string titulo = TextoTituloTarea.Text.Trim();
        if (string.IsNullOrWhiteSpace(titulo))
        {
            MostrarError("El título es obligatorio.");
            return;
        }

        if (!DateOnly.TryParseExact(TextoFecha.Text.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly fecha))
        {
            MostrarError("Fecha inválida. Usa el formato dd/mm/aaaa.");
            return;
        }

        if (!TimeOnly.TryParseExact(TextoHora.Text.Trim(), new[] { "H:mm", "HH:mm" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeOnly hora))
        {
            MostrarError("Hora inválida. Usa formato 24 horas, ej: 19:30.");
            return;
        }

        DateTime fechaEntrega = fecha.ToDateTime(hora);

        int? cursoId = (ComboCurso.SelectedItem as CursoOpcionViewModel)?.CursoId;

        PrioridadTarea prioridad = ComboPrioridad.SelectedIndex switch
        {
            0 => PrioridadTarea.Baja,
            2 => PrioridadTarea.Alta,
            _ => PrioridadTarea.Media
        };

        string? descripcion = string.IsNullOrWhiteSpace(TextoDescripcion.Text) ? null : TextoDescripcion.Text.Trim();

        int tareaId;

        if (_tareaExistente is null)
        {
            tareaId = _tareaRepository.Crear(new Tarea
            {
                UsuarioId = _usuarioId,
                CursoId = cursoId,
                Titulo = titulo,
                Descripcion = descripcion,
                FechaEntrega = fechaEntrega,
                Prioridad = prioridad
            });
        }
        else
        {
            tareaId = _tareaExistente.TareaId;
            _tareaExistente.CursoId = cursoId;
            _tareaExistente.Titulo = titulo;
            _tareaExistente.Descripcion = descripcion;
            _tareaExistente.FechaEntrega = fechaEntrega;
            _tareaExistente.Prioridad = prioridad;

            if (!_tareaRepository.Actualizar(_tareaExistente))
            {
                MostrarError("No se pudo guardar (los datos cambiaron en otro lugar). Vuelve a intentarlo.");
                return;
            }
        }

        var idsActuales = _recordatorios.Where(r => r.RecordatorioTareaId is not null)
            .Select(r => r.RecordatorioTareaId!.Value).ToHashSet();

        foreach (long idEliminado in _recordatorioIdsOriginales.Except(idsActuales))
        {
            _recordatorioRepository.Eliminar(idEliminado);
        }

        foreach (var nuevo in _recordatorios.Where(r => r.RecordatorioTareaId is null))
        {
            if (fechaEntrega.AddMinutes(-nuevo.MinutosAntelacion) > DateTime.Now)
            {
                _recordatorioRepository.Crear(tareaId, nuevo.MinutosAntelacion);
            }
        }

        DialogResult = true;
        Close();
    }

    private void BotonCancelar_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void MostrarError(string mensaje)
    {
        TextoError.Text = mensaje;
        TextoError.Visibility = Visibility.Visible;
    }
}