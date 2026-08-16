using MarcTime.Core.Deteccion;
using MarcTime.Data;
using MarcTime.Data.Conexion;
using MarcTime.Data.Repositories;
using MarcTime.UI.Servicios;
using MarcTime.UI.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Windows;
using System.Windows.Threading;

namespace MarcTime.UI;

public partial class App : Application
{
    private MonitorUsoService? _monitorUso;
    private DispatcherTimer? _timerMonitoreo;
    private DispatcherTimer? _timerLimites;
    private DispatcherTimer? _timerNotificaciones;
    private DispatcherTimer? _timerRestriccionesHorario;
    private ServicioNotificaciones? _servicioNotificaciones;
    private MainWindow? _mainWindow;

    private const int UsuarioActivoId = 1;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DapperConfiguracion.Registrar();
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var configuracion = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        string cadenaConexion = configuracion.GetConnectionString("MarcTimeDB")
            ?? throw new InvalidOperationException("Falta la cadena de conexion 'MarcTimeDB' en appsettings.");

        var fabricaConexion = new ConexionFactory(cadenaConexion);

        // --- Seccion 6: monitoreo de uso ---
        _monitorUso = new MonitorUsoService(
            detector: new DetectorAppActiva(),
            aplicacionRepository: new AplicacionRepository(fabricaConexion),
            sesionUsoRepository: new SesionUsoRepository(fabricaConexion),
            usuarioId: UsuarioActivoId);
        _timerMonitoreo = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        _timerMonitoreo.Tick += (_, _) => _monitorUso.Muestrear();
        _timerMonitoreo.Start();

        // --- Seccion 8: limites de tiempo diarios ---
        var gestorLimites = new GestorLimitesTiempoService(new AplicacionRepository(fabricaConexion), UsuarioActivoId);
        _timerLimites = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        _timerLimites.Tick += (_, _) => gestorLimites.RevisarLimites();
        _timerLimites.Start();

        // --- Seccion 14: apps restringidas a bloques de horario ---
        var gestorRestricciones = new GestorRestriccionesHorarioService(new RestriccionHorarioAppRepository(fabricaConexion), UsuarioActivoId);
        _timerRestriccionesHorario = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        _timerRestriccionesHorario.Tick += (_, _) => gestorRestricciones.RevisarRestricciones();
        _timerRestriccionesHorario.Start();

        // --- Seccion 11, 12, 14: notificaciones de bandeja + sonido + recordatorios ---
        _servicioNotificaciones = new ServicioNotificaciones(
            horarioRepository: new HorarioClaseRepository(fabricaConexion),
            notificacionRepository: new NotificacionRepository(fabricaConexion),
            configuracionNotificacionRepository: new ConfiguracionNotificacionRepository(fabricaConexion),
            recordatorioTareaRepository: new RecordatorioTareaRepository(fabricaConexion),
            usuarioId: UsuarioActivoId);
        _timerNotificaciones = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
        _timerNotificaciones.Tick += (_, _) => _servicioNotificaciones.RevisarYNotificar();
        _timerNotificaciones.Start();

        // --- Seccion 15: ventana principal y navegacion ---
        var mainViewModel = new MainViewModel(
            inicio: new InicioViewModel(new ReporteUsoRepository(fabricaConexion), UsuarioActivoId),
            horario: new HorarioViewModel(
                new HorarioClaseRepository(fabricaConexion),
                new CursoRepository(fabricaConexion),
                new AplicacionRepository(fabricaConexion),
                new RestriccionHorarioAppRepository(fabricaConexion),
                UsuarioActivoId),
            tareas: new TareasViewModel(
                new TareaRepository(fabricaConexion),
                new CursoRepository(fabricaConexion),
                new RecordatorioTareaRepository(fabricaConexion),
                UsuarioActivoId),
            historial: new HistorialViewModel(new ReporteUsoRepository(fabricaConexion), UsuarioActivoId),
            configuracion: new ConfiguracionViewModel(
                new TipoEventoRepository(fabricaConexion),
                new SonidoRepository(fabricaConexion),
                new ConfiguracionNotificacionRepository(fabricaConexion),
                new CursoRepository(fabricaConexion),
                new ReproductorSonidoService(),
                UsuarioActivoId));

        _mainWindow = new MainWindow { DataContext = mainViewModel };
        _mainWindow.Show();
        MainWindow = _mainWindow;

        _servicioNotificaciones.AbrirSolicitado += () =>
        {
            _mainWindow.Show();
            _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.Activate();
        };
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        RegistradorErrores.Registrar(e.Exception, "Excepcion no controlada en el hilo de la interfaz");

        string mensaje = e.Exception is SqlException or InvalidOperationException
            ? "Hubo un problema de conexión con la base de datos. Verifica que SQL Server esté disponible e intenta de nuevo."
            : "Ocurrió un error inesperado. Se registró el detalle en el log de MARC TIME.";

        MessageBox.Show(mensaje, "MARC TIME", MessageBoxButton.OK, MessageBoxImage.Error);

        // Marca la excepcion como manejada: SIN esto, WPF cierra toda la
        // app de golpe apenas termina este metodo (era el comportamiento
        // que vimos en las Secciones 6 y 9 con los SqlException).
        e.Handled = true;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _timerMonitoreo?.Stop();
        _timerLimites?.Stop();
        _timerRestriccionesHorario?.Stop();
        _timerNotificaciones?.Stop();
        _monitorUso?.DetenerYCerrarSesionActual();
        _servicioNotificaciones?.Dispose();
        base.OnExit(e);
    }
}