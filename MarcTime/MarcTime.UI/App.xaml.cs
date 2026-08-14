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

    //BORRAR EN UI
    private void EjecutarPruebaTareas(ITareaRepository tareaRepository)
    {
        int tareaId = tareaRepository.Crear(new Core.Models.Tareas.Tarea
        {
            UsuarioId = UsuarioActivoId,
            Titulo = "Entregar informe de avance",
            Descripcion = "Seccion 10 de la hoja de ruta",
            FechaEntrega = DateTime.Now.AddDays(3),
            Prioridad = Core.Models.Enums.PrioridadTarea.Alta
        });
        Debug.WriteLine($"CREATE Tarea -> Id generado: {tareaId}");

        var pendientes = tareaRepository.ObtenerPendientes(UsuarioActivoId);
        Debug.WriteLine($"READ (pendientes) -> {pendientes.Count} tarea(s)");

        var proximas = tareaRepository.ObtenerProximasAVencer(UsuarioActivoId, diasAntelacion: 7);
        Debug.WriteLine($"READ (proximas a vencer, 7 dias) -> {proximas.Count} tarea(s)");

        var tarea = tareaRepository.ObtenerPorId(tareaId)!;
        tarea.Descripcion = "Descripcion actualizada";
        bool actualizada = tareaRepository.Actualizar(tarea);
        Debug.WriteLine($"UPDATE -> {(actualizada ? "OK" : "FALLO")}");

        bool marcada = tareaRepository.MarcarCompletada(tareaId, completada: true);
        Debug.WriteLine($"MARCAR COMPLETADA -> {(marcada ? "OK" : "FALLO")}");

        bool eliminada = tareaRepository.Eliminar(tareaId);
        Debug.WriteLine($"DELETE -> {(eliminada ? "OK" : "FALLO")}");
    }


    //BORRABLE EN UI
    private void EjecutarPruebaHorarios(ICursoRepository cursoRepository, IHorarioClaseRepository horarioRepository)
    {
        int cursoId = cursoRepository.Crear(new Core.Models.Academico.Curso
        {
            UsuarioId = UsuarioActivoId,
            Nombre = "Arquitectura de Software",
            Codigo = "ARQ-101",
            Color = "#4A90D9"
        });
        Debug.WriteLine($"CREATE Curso -> Id generado: {cursoId}");

        int horarioId = horarioRepository.Crear(new Core.Models.Academico.HorarioClase
        {
            CursoId = cursoId,
            DiaSemana = Core.Models.Enums.DiaSemana.Lunes,
            HoraInicio = new TimeOnly(8, 0),
            HoraFin = new TimeOnly(10, 0),
            Ubicacion = "Aula 301"
        });
        Debug.WriteLine($"CREATE HorarioClase -> Id generado: {horarioId}");

        var horarios = horarioRepository.ObtenerPorCurso(cursoId);
        Debug.WriteLine($"READ -> {horarios.Count} horario(s) para el curso");

        var horario = horarioRepository.ObtenerPorId(horarioId)!;
        horario.Ubicacion = "Aula 405 (cambio de salón)";
        bool actualizado = horarioRepository.Actualizar(horario);
        Debug.WriteLine($"UPDATE -> {(actualizado ? "OK" : "FALLO")}");

        bool eliminado = horarioRepository.Eliminar(horarioId);
        Debug.WriteLine($"DELETE HorarioClase -> {(eliminado ? "OK" : "FALLO")}");

        cursoRepository.Eliminar(cursoId);
        Debug.WriteLine("DELETE Curso -> OK (limpieza de la prueba)");
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        
        base.OnStartup(e);

        MarcTime.Data.DapperConfiguracion.Registrar();

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
        var aplicacionRepository = new AplicacionRepository(fabricaConexion);
        var gestorLimites = new GestorLimitesTiempoService(aplicacionRepository, UsuarioActivoId);

        //AGREGADO 9
        var timerPrueba9 = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        timerPrueba9.Tick += (_, _) =>
        {
            timerPrueba9.Stop();
            EjecutarPruebaHorarios(new CursoRepository(fabricaConexion), new HorarioClaseRepository(fabricaConexion));
        };
        timerPrueba9.Start();


        //AGREGADO 10
        var timerPrueba10 = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        timerPrueba10.Tick += (_, _) =>
        {
            timerPrueba10.Stop();
            EjecutarPruebaTareas(new TareaRepository(fabricaConexion));
        };
        timerPrueba10.Start();


        var timerLimites = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        timerLimites.Tick += (_, _) => gestorLimites.RevisarLimites();
        timerLimites.Start();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _timerMonitoreo?.Stop();
        _monitorUso?.DetenerYCerrarSesionActual();
        base.OnExit(e);
    }
}