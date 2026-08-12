using MarcTime.Core.Deteccion;
using MarcTime.Data.Conexion;
using MarcTime.Data.Repositories;
using MarcTime.UI.Servicios;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace MarcTime.UI;

public partial class App : Application
{
    private MonitorUsoService? _monitorUso;
    private DispatcherTimer? _timerMonitoreo;

    // TODO: reemplazar por el usuario real cuando exista login/seleccion de usuario.
    private const int UsuarioActivoId = 1;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var configuracion = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        string cadenaConexion = configuracion.GetConnectionString("MarcTimeDB")
            ?? throw new InvalidOperationException("Falta la cadena de conexion 'MarcTimeDB' en appsettings.");

        var fabricaConexion = new ConexionFactory(cadenaConexion);

        _monitorUso = new MonitorUsoService(
            detector: new DetectorAppActiva(),
            aplicacionRepository: new AplicacionRepository(fabricaConexion),
            sesionUsoRepository: new SesionUsoRepository(fabricaConexion),
            usuarioId: UsuarioActivoId);

        _timerMonitoreo = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        _timerMonitoreo.Tick += (_, _) => _monitorUso.Muestrear();
        _timerMonitoreo.Start();

        //AÑADIDO SECCIÓN 7
        var timerPrueba = new DispatcherTimer { Interval = TimeSpan.FromSeconds(8) };
        timerPrueba.Tick += (_, _) =>
        {
            timerPrueba.Stop();
            EjecutarPruebaLimites(new AplicacionRepository(fabricaConexion));
        };
        timerPrueba.Start();
    }

    //AGREGANDO MÉTODO NUEVO SECCIÓN 7
    private void EjecutarPruebaLimites(IAplicacionRepository aplicacionRepository)
    {
        // Toma la primera app con uso registrado hoy para la prueba
        var estados = aplicacionRepository.ObtenerEstadoLimites(UsuarioActivoId);
        if (estados.Count == 0)
        {
            Debug.WriteLine("Prueba Seccion 7: no hay apps registradas todavia.");
            return;
        }

        var primeraApp = estados[0];
        Debug.WriteLine($"Antes -> {primeraApp.NombreVisible}: usado {primeraApp.MinutosUsadosHoy} min, limite {primeraApp.LimiteMinutosDiarios?.ToString() ?? "sin limite"}");

        aplicacionRepository.EstablecerLimiteMinutosDiarios(primeraApp.AplicacionId, 60);

        var estadosActualizados = aplicacionRepository.ObtenerEstadoLimites(UsuarioActivoId);
        var appActualizada = estadosActualizados.First(a => a.AplicacionId == primeraApp.AplicacionId);
        Debug.WriteLine($"Despues -> {appActualizada.NombreVisible}: limite {appActualizada.LimiteMinutosDiarios} min, restantes {appActualizada.MinutosRestantes} min, alcanzado: {appActualizada.LimiteAlcanzado}");
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _timerMonitoreo?.Stop();
        _monitorUso?.DetenerYCerrarSesionActual();
        base.OnExit(e);
    }
}