using System.Collections.ObjectModel;
using System.Windows.Input;
using MarcTime.Data.Repositories;
using MarcTime.UI.Comun;
using MarcTime.UI.Servicios;

namespace MarcTime.UI.ViewModels;

/// <summary>
/// Pantalla de configuracion: sonido por tipo de evento (Seccion 12, ahora
/// con UI real) + administracion de cursos (renombrar/eliminar, para que
/// los desplegables de Horario y Tareas queden limpios de pruebas viejas).
/// </summary>
public class ConfiguracionViewModel : ViewModelBase
{
    private readonly ITipoEventoRepository _tipoEventoRepository;
    private readonly ISonidoRepository _sonidoRepository;
    private readonly IConfiguracionNotificacionRepository _configuracionNotificacionRepository;
    private readonly ICursoRepository _cursoRepository;
    private readonly ReproductorSonidoService _reproductorSonido;
    private readonly int _usuarioId;
    private string? _mensajeEstado;

    public ConfiguracionViewModel(
        ITipoEventoRepository tipoEventoRepository,
        ISonidoRepository sonidoRepository,
        IConfiguracionNotificacionRepository configuracionNotificacionRepository,
        ICursoRepository cursoRepository,
        ReproductorSonidoService reproductorSonido,
        int usuarioId)
    {
        _tipoEventoRepository = tipoEventoRepository;
        _sonidoRepository = sonidoRepository;
        _configuracionNotificacionRepository = configuracionNotificacionRepository;
        _cursoRepository = cursoRepository;
        _reproductorSonido = reproductorSonido;
        _usuarioId = usuarioId;

        GuardarSonidosCommand = new RelayCommand(GuardarSonidos);
        GuardarCursoCommand = new RelayCommand<int>(GuardarCurso);
        EliminarCursoCommand = new RelayCommand<int>(EliminarCurso);

        Cargar();
    }

    public ObservableCollection<ConfiguracionEventoViewModel> EventosSonido { get; } = new();
    public ObservableCollection<CursoAdminViewModel> Cursos { get; } = new();

    public string? MensajeEstado
    {
        get => _mensajeEstado;
        set => SetProperty(ref _mensajeEstado, value);
    }

    public ICommand GuardarSonidosCommand { get; }
    public ICommand GuardarCursoCommand { get; }
    public ICommand EliminarCursoCommand { get; }

    private void Cargar()
    {
        var sonidosDisponibles = _sonidoRepository.ObtenerDisponibles(_usuarioId);

        EventosSonido.Clear();
        foreach (var tipoEvento in _tipoEventoRepository.ObtenerTodos())
        {
            var configuracionActual = _configuracionNotificacionRepository.ObtenerConfiguracion(_usuarioId, tipoEvento.TipoEventoId);

            var item = new ConfiguracionEventoViewModel(
                tipoEvento.TipoEventoId,
                NombreAmigable(tipoEvento.Codigo),
                sonidosDisponibles,
                probarSonido: ruta => _reproductorSonido.Reproducir(ruta));

            item.SonidoSeleccionado = configuracionActual?.SonidoId is int sonidoId
                ? sonidosDisponibles.FirstOrDefault(s => s.SonidoId == sonidoId)
                : sonidosDisponibles.FirstOrDefault(s => s.EsPredeterminado);
            item.Activo = configuracionActual?.Activo ?? true;

            EventosSonido.Add(item);
        }

        Cursos.Clear();
        foreach (var curso in _cursoRepository.ObtenerTodos(_usuarioId))
        {
            Cursos.Add(new CursoAdminViewModel(curso));
        }
    }

    private static string NombreAmigable(string codigo) => codigo switch
    {
        "ClaseProxima" => "Clase próxima",
        "TareaProxima" => "Recordatorio de tarea",
        _ => codigo
    };

    private void GuardarSonidos()
    {
        foreach (var evento in EventosSonido)
        {
            _configuracionNotificacionRepository.Establecer(new Core.Models.Notificaciones.ConfiguracionNotificacion
            {
                UsuarioId = _usuarioId,
                TipoEventoId = evento.TipoEventoId,
                SonidoId = evento.SonidoSeleccionado?.SonidoId,
                MinutosAntelacion = 10,
                Activo = evento.Activo
            });
        }

        MensajeEstado = "Preferencias de sonido guardadas.";
    }

    private void GuardarCurso(int cursoId)
    {
        var item = Cursos.FirstOrDefault(c => c.CursoId == cursoId);
        if (item is null) return;

        item.Curso.Nombre = item.Nombre;
        _cursoRepository.Actualizar(item.Curso);
        MensajeEstado = $"\"{item.Nombre}\" actualizado.";
    }

    private void EliminarCurso(int cursoId)
    {
        try
        {
            _cursoRepository.Eliminar(cursoId);
            var item = Cursos.FirstOrDefault(c => c.CursoId == cursoId);
            if (item is not null) Cursos.Remove(item);
            MensajeEstado = "Curso eliminado.";
        }
        catch (Exception)
        {
            MensajeEstado = "No se pudo eliminar: el curso tiene horarios o tareas asociadas. Quítalas primero.";
        }
    }
}