using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using MarcTime.Core.Consultas;
using MarcTime.Core.Models.Academico;
using MarcTime.Core.Models.Enums;
using MarcTime.Data.Repositories;
using MarcTime.UI.ViewModels;

namespace MarcTime.UI.Vistas;

/// <summary>
/// Dialogo de bloque de horario. Sirve tanto para crear como para editar:
/// si se pasa "bloqueExistente", precarga los campos y al guardar hace
/// Actualizar() en vez de Crear() - misma ventana, misma validacion.
/// </summary>
public partial class EditarBloqueWindow : Window
{
    private readonly DiaSemana _dia;
    private readonly ICursoRepository _cursoRepository;
    private readonly IAplicacionRepository _aplicacionRepository;
    private readonly IRestriccionHorarioAppRepository _restriccionRepository;
    private readonly IHorarioClaseRepository _horarioRepository;
    private readonly int _usuarioId;
    private readonly BloqueHorarioDetalle? _bloqueExistente;
    private List<int> _appsOriginalmenteAsignadas = new();

    public EditarBloqueWindow(
        DiaSemana dia,
        ICursoRepository cursoRepository,
        IAplicacionRepository aplicacionRepository,
        IRestriccionHorarioAppRepository restriccionRepository,
        IHorarioClaseRepository horarioRepository,
        int usuarioId,
        BloqueHorarioDetalle? bloqueExistente = null)
    {
        InitializeComponent();

        _dia = dia;
        _cursoRepository = cursoRepository;
        _aplicacionRepository = aplicacionRepository;
        _restriccionRepository = restriccionRepository;
        _horarioRepository = horarioRepository;
        _usuarioId = usuarioId;
        _bloqueExistente = bloqueExistente;

        var cursos = _cursoRepository.ObtenerTodos(usuarioId);
        ComboCursos.ItemsSource = cursos;

        var apps = _aplicacionRepository.ObtenerTodas(usuarioId)
            .Select(a => new AppSeleccionableViewModel(a))
            .ToList();
        ListaApps.ItemsSource = apps;

        if (_bloqueExistente is null)
        {
            TextoDia.Text = $"Nuevo bloque - {dia}";
        }
        else
        {
            TextoDia.Text = $"Editar bloque - {dia}";
            PrecargarDatos(_bloqueExistente, cursos, apps);
        }
    }

    private void PrecargarDatos(BloqueHorarioDetalle bloque, List<Curso> cursos, List<AppSeleccionableViewModel> apps)
    {
        ComboCursos.SelectedItem = cursos.FirstOrDefault(c => c.CursoId == bloque.CursoId);

        TextoHoraInicio.Text = bloque.HoraInicio.ToString("H\\:mm");
        TextoHoraFin.Text = bloque.HoraFin.ToString("H\\:mm");
        PlaceholderHoraInicio.Visibility = Visibility.Collapsed;
        PlaceholderHoraFin.Visibility = Visibility.Collapsed;

        TextoUbicacion.Text = bloque.Ubicacion ?? string.Empty;

        _appsOriginalmenteAsignadas = _restriccionRepository.ObtenerAplicacionIdsPorHorario(bloque.HorarioClaseId);
        foreach (var app in apps.Where(a => _appsOriginalmenteAsignadas.Contains(a.Aplicacion.AplicacionId)))
        {
            app.Seleccionada = true;
        }
    }

    private void CampoHora_GotFocus(object sender, RoutedEventArgs e)
    {
        ObtenerPlaceholderDe((TextBox)sender).Visibility = Visibility.Collapsed;
    }

    private void CampoHora_LostFocus(object sender, RoutedEventArgs e)
    {
        var caja = (TextBox)sender;
        ObtenerPlaceholderDe(caja).Visibility = string.IsNullOrWhiteSpace(caja.Text) ? Visibility.Visible : Visibility.Collapsed;
    }

    private TextBlock ObtenerPlaceholderDe(TextBox caja) =>
        caja == TextoHoraInicio ? PlaceholderHoraInicio : PlaceholderHoraFin;

    private void BotonGuardar_Click(object sender, RoutedEventArgs e)
    {
        TextoError.Visibility = Visibility.Collapsed;

        if (!TryParsearHora(TextoHoraInicio.Text, out TimeOnly horaInicio))
        {
            MostrarError("Hora de inicio inválida. Usa formato 24 horas, ej: 7:00 o 19:30.");
            return;
        }

        if (!TryParsearHora(TextoHoraFin.Text, out TimeOnly horaFin))
        {
            MostrarError("Hora de fin inválida. Usa formato 24 horas, ej: 7:00 o 19:30.");
            return;
        }

        if (horaFin <= horaInicio)
        {
            MostrarError("La hora de fin debe ser posterior a la hora de inicio.");
            return;
        }

        string nombreNuevoCurso = TextoNuevoCurso.Text.Trim();
        var cursoExistente = ComboCursos.SelectedItem as Curso;

        if (cursoExistente is null && string.IsNullOrWhiteSpace(nombreNuevoCurso))
        {
            MostrarError("Elige un curso existente o escribe uno nuevo.");
            return;
        }

        int cursoId = cursoExistente?.CursoId ?? _cursoRepository.Crear(new Curso
        {
            UsuarioId = _usuarioId,
            Nombre = nombreNuevoCurso
        });

        int horarioClaseId;

        if (_bloqueExistente is null)
        {
            horarioClaseId = _horarioRepository.Crear(new HorarioClase
            {
                CursoId = cursoId,
                DiaSemana = _dia,
                HoraInicio = horaInicio,
                HoraFin = horaFin,
                Ubicacion = string.IsNullOrWhiteSpace(TextoUbicacion.Text) ? null : TextoUbicacion.Text.Trim()
            });
        }
        else
        {
            horarioClaseId = _bloqueExistente.HorarioClaseId;
            _horarioRepository.Actualizar(new HorarioClase
            {
                HorarioClaseId = horarioClaseId,
                CursoId = cursoId,
                DiaSemana = _dia,
                HoraInicio = horaInicio,
                HoraFin = horaFin,
                Ubicacion = string.IsNullOrWhiteSpace(TextoUbicacion.Text) ? null : TextoUbicacion.Text.Trim()
            });
        }

        var idsSeleccionados = ((List<AppSeleccionableViewModel>)ListaApps.ItemsSource)
            .Where(a => a.Seleccionada)
            .Select(a => a.Aplicacion.AplicacionId)
            .ToHashSet();

        // En modo edicion: agrega las nuevas marcadas, quita las que se desmarcaron.
        // En modo creacion, _appsOriginalmenteAsignadas esta vacia, asi que solo agrega.
        foreach (int id in idsSeleccionados.Except(_appsOriginalmenteAsignadas))
        {
            _restriccionRepository.Asignar(horarioClaseId, id);
        }
        foreach (int id in _appsOriginalmenteAsignadas.Except(idsSeleccionados))
        {
            _restriccionRepository.Quitar(horarioClaseId, id);
        }

        DialogResult = true;
        Close();
    }

    private static bool TryParsearHora(string texto, out TimeOnly hora)
    {
        string[] formatosAceptados = { "H:mm", "HH:mm" };
        return TimeOnly.TryParseExact(texto.Trim(), formatosAceptados, CultureInfo.InvariantCulture,
            DateTimeStyles.None, out hora);
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