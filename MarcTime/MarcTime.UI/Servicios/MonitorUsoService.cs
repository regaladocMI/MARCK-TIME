using System.Diagnostics;
using MarcTime.Core.Deteccion;
using MarcTime.Data.Repositories;

namespace MarcTime.UI.Servicios;

/// <summary>
/// Junta el detector de app activa (Core) con los repositorios (Data) para
/// llevar el registro real de uso. Solo abre una sesion nueva cuando la app
/// activa CAMBIA respecto al ultimo muestreo; mientras el usuario se queda
/// en la misma app, la sesion sigue abierta.
///
/// Antes de abrir una sesion nueva para una app, verifica si ya existe una
/// abierta (ObtenerSesionAbierta): puede pasar si en una ejecucion anterior
/// la app se cerro de forma abrupta (Kill(), boton "Detener" de VS, crash)
/// sin pasar por OnExit -> queda una sesion "huerfana" con FechaHoraFin NULL.
/// El indice unico filtrado de SesionesUso (Seccion 2) no permite 2 sesiones
/// abiertas para la misma app, asi que sin esta verificacion el INSERT
/// revienta con una excepcion de clave duplicada.
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

    public void Muestrear()
    {
        AppActivaInfo? appActiva = _detector.ObtenerAppActiva();
        if (appActiva is null)
        {
            return;
        }

        var aplicacion = _aplicacionRepository.ObtenerOCrearPorNombreEjecutable(
            _usuarioId, appActiva.NombreEjecutable, appActiva.NombreEjecutable);

        if (aplicacion.AplicacionId == _aplicacionActivaId)
        {
            return;
        }

        DateTime ahora = DateTime.Now;

        if (_sesionAbiertaId is not null)
        {
            _sesionUsoRepository.CerrarSesion(_sesionAbiertaId.Value, ahora);
            Debug.WriteLine($"Sesion cerrada -> AplicacionId: {_aplicacionActivaId}, SesionUsoId: {_sesionAbiertaId}");
        }

        var sesionHuerfana = _sesionUsoRepository.ObtenerSesionAbierta(aplicacion.AplicacionId);
        if (sesionHuerfana is not null)
        {
            Debug.WriteLine($"Sesion huerfana adoptada -> {aplicacion.NombreVisible} (AplicacionId: {aplicacion.AplicacionId}, SesionUsoId: {sesionHuerfana.SesionUsoId})");
            _sesionAbiertaId = sesionHuerfana.SesionUsoId;
        }
        else
        {
            _sesionAbiertaId = _sesionUsoRepository.AbrirSesion(aplicacion.AplicacionId, ahora, appActiva.TituloVentana);
            Debug.WriteLine($"Sesion abierta -> {aplicacion.NombreVisible} (AplicacionId: {aplicacion.AplicacionId}, SesionUsoId: {_sesionAbiertaId})");
        }

        _aplicacionActivaId = aplicacion.AplicacionId;
    }

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