using MarcTime.Core.Deteccion;
using MarcTime.Data;
using MarcTime.Data.Conexion;
using MarcTime.Data.Repositories;
using MarcTime.UI.Servicios;
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
    private ServicioNotificaciones? _servicioNotificaciones;

    // TODO: reemplazar por el usuario real cuando exista login/seleccion de usuario.
    private const int UsuarioActivoId = 1;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DapperConfiguracion.Registrar();

        // La app ahora vive en la bandeja del sistema: cerrar la ventana
        // principal NO debe terminar el proceso (solo "Salir" desde el menu
        // del icono de bandeja lo hace). ShutdownMode por defecto es
        // OnLastWindowClose; lo cambiamos para permitir seguir en segundo plano.
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

        // --- Seccion 8: limites de tiempo (aviso + cuenta regresiva + cierre) ---
        var gestorLimites = new GestorLimitesTiempoService(new AplicacionRepository(fabricaConexion), UsuarioActivoId);

        _timerLimites = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        _timerLimites.Tick += (_, _) => gestorLimites.RevisarLimites();
        _timerLimites.Start();

        // --- Seccion 11: notificaciones de bandeja (horarios y tareas) ---
        _servicioNotificaciones = new ServicioNotificaciones(
            horarioRepository: new HorarioClaseRepository(fabricaConexion),
            tareaRepository: new TareaRepository(fabricaConexion),
            notificacionRepository: new NotificacionRepository(fabricaConexion),
            configuracionNotificacionRepository: new ConfiguracionNotificacionRepository(fabricaConexion),
            usuarioId: UsuarioActivoId);

        _timerNotificaciones = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) }; //cambiado a 10 segundos para pruebas, en producción puede ser más largo
        _timerNotificaciones.Tick += (_, _) => _servicioNotificaciones.RevisarYNotificar();
        _timerNotificaciones.Start();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _timerMonitoreo?.Stop();
        _timerLimites?.Stop();
        _timerNotificaciones?.Stop();
        _monitorUso?.DetenerYCerrarSesionActual();
        _servicioNotificaciones?.Dispose();
        base.OnExit(e);
    }
}