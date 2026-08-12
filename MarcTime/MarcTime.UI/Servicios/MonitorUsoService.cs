using System.Diagnostics;
using MarcTime.Core.Deteccion;
using MarcTime.Data.Repositories;

namespace MarcTime.UI.Servicios;

/// <summary>
/// Junta el detector de app activa (Core) con los repositorios (Data) para
/// llevar el registro real de uso. Solo abre una sesion nueva cuando la app
/// activa CAMBIA respecto al ultimo muestreo; mientras el usuario se queda
/// en la misma app, la sesion sigue abierta.
/// </summary>
public class MonitorUsoService
{
    private readonly IDetectorAppActiva _detector;
    private readonly IAplicacionRepository _aplicacionRepository;
    private readonly ISesionUsoRepository _sesionUsoRepository;
    private readonly int _usuarioId;

    private int? _aplicacionActivaId;
    private long? _sesionAbiertaId;

    public MonitorUsoService(
        IDetectorAppActiva detector,
        IAplicacionRepository aplicacionRepository,
        ISesionUsoRepository sesionUsoRepository,
        int usuarioId)
    {
        _detector = detector;
        _aplicacionRepository = aplicacionRepository;
        _sesionUsoRepository = sesionUsoRepository;
        _usuarioId = usuarioId;
    }

    /// <summary>
    /// Se llama en cada "tick" del timer. Detecta la app activa y, si cambio
    /// desde el ultimo muestreo, cierra la sesion anterior y abre una nueva.
    /// </summary>
    public void Muestrear()
    {
        AppActivaInfo? appActiva = _detector.ObtenerAppActiva();
        if (appActiva is null)
        {
            // Deteccion fallida momentanea (ver Seccion 5): no cerramos ni
            // abrimos nada, se mantiene la sesion que ya estaba abierta.
            return;
        }

        var aplicacion = _aplicacionRepository.ObtenerOCrearPorNombreEjecutable(
            _usuarioId, appActiva.NombreEjecutable, appActiva.NombreEjecutable);

        if (aplicacion.AplicacionId == _aplicacionActivaId)
        {
            return; // sigue en la misma app, no hay nada que hacer
        }

        DateTime ahora = DateTime.Now;

        if (_sesionAbiertaId is not null)
        {
            _sesionUsoRepository.CerrarSesion(_sesionAbiertaId.Value, ahora);
            Debug.WriteLine($"Sesion cerrada -> AplicacionId: {_aplicacionActivaId}, SesionUsoId: {_sesionAbiertaId}");
        }

        _sesionAbiertaId = _sesionUsoRepository.AbrirSesion(aplicacion.AplicacionId, ahora, appActiva.TituloVentana);
        _aplicacionActivaId = aplicacion.AplicacionId;
        Debug.WriteLine($"Sesion abierta -> {aplicacion.NombreVisible} (AplicacionId: {aplicacion.AplicacionId}, SesionUsoId: {_sesionAbiertaId})");
    }

    /// <summary>
    /// Cierra la sesion actualmente abierta. Debe llamarse al cerrar la
    /// aplicacion (OnExit) para no dejar sesiones huerfanas.
    /// </summary>
    public void DetenerYCerrarSesionActual()
    {
        if (_sesionAbiertaId is not null)
        {
            _sesionUsoRepository.CerrarSesion(_sesionAbiertaId.Value, DateTime.Now);
            Debug.WriteLine($"Sesion cerrada por cierre de aplicacion -> SesionUsoId: {_sesionAbiertaId}");
            _sesionAbiertaId = null;
        }
    }
}