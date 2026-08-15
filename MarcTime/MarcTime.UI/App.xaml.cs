using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.Configuration;
using MarcTime.Core.Deteccion;
using MarcTime.Data;
using MarcTime.Data.Conexion;
using MarcTime.Data.Repositories;
using MarcTime.UI.Servicios;

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

        // La app vive en la bandeja del sistema: cerrar la ventana principal
        // NO debe terminar el proceso (solo "Salir" desde el menu del icono
        // de bandeja lo hace).
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

        // --- Seccion 11 y 12: notificaciones de bandeja + sonido ---
        _servicioNotificaciones = new ServicioNotificaciones(
            horarioRepository: new HorarioClaseRepository(fabricaConexion),
            tareaRepository: new TareaRepository(fabricaConexion),
            notificacionRepository: new NotificacionRepository(fabricaConexion),
            configuracionNotificacionRepository: new ConfiguracionNotificacionRepository(fabricaConexion),
            usuarioId: UsuarioActivoId);

        _timerNotificaciones = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
        _timerNotificaciones.Tick += (_, _) => _servicioNotificaciones.RevisarYNotificar();
        _timerNotificaciones.Start();

        // --- Seccion 13: prueba temporal de reportes de uso ---
        var timerPrueba13 = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        timerPrueba13.Tick += (_, _) =>
        {
            timerPrueba13.Stop();
            EjecutarPruebaReportes(new ReporteUsoRepository(fabricaConexion));
        };
        timerPrueba13.Start();
    }

    // --- Seccion 13: prueba temporal, borrar cuando exista una pantalla real de reportes ---
    private void EjecutarPruebaReportes(IReporteUsoRepository reporteRepository)
    {
        DateOnly hoy = DateOnly.FromDateTime(DateTime.Now);
        DateOnly hace7Dias = hoy.AddDays(-7);

        var porApp = reporteRepository.ObtenerUsoPorApp(UsuarioActivoId, hace7Dias, hoy);
        Debug.WriteLine($"--- Uso por app (ultimos 7 dias): {porApp.Count} app(s) ---");
        foreach (var app in porApp)
        {
            Debug.WriteLine($"  {app.NombreVisible} [{app.NombreCategoria ?? "sin categoria"}]: {app.MinutosTotales} min");
        }

        var porDia = reporteRepository.ObtenerUsoPorDia(UsuarioActivoId, hace7Dias, hoy);
        Debug.WriteLine($"--- Uso por dia: {porDia.Count} dia(s) con registro ---");
        foreach (var dia in porDia)
        {
            Debug.WriteLine($"  {dia.Fecha}: {dia.MinutosTotales} min");
        }

        var productividad = reporteRepository.ObtenerResumenProductividad(UsuarioActivoId, hace7Dias, hoy);
        Debug.WriteLine($"--- Productividad: {productividad.PorcentajeProductivo}% productivo ---");
        Debug.WriteLine($"  Productivo: {productividad.MinutosProductivos} min | No productivo: {productividad.MinutosNoProductivos} min | Sin categoria: {productividad.MinutosSinCategoria} min");
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