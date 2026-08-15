using System.Diagnostics;
using System.Windows;
using MarcTime.Core.Models.Notificaciones;
using MarcTime.Data.Repositories;
using WinForms = System.Windows.Forms;

namespace MarcTime.UI.Servicios;

/// <summary>
/// Dueño del icono de bandeja del sistema. Revisa periodicamente horarios
/// proximos (Seccion 11) y recordatorios de tarea pendientes (Seccion 14,
/// reemplaza el aviso unico de 24h por avisos escalonados configurables por
/// tarea) y muestra un globo + sonido por cada evento nuevo.
///
/// IDs de TiposEvento esperados en la BD: 1 = ClaseProxima, 2 = TareaProxima
/// </summary>
public class ServicioNotificaciones : IDisposable
{
    private const int MinutosAntelacionClase = 15;
    private const int TipoEventoClaseProxima = 1;
    private const int TipoEventoTareaProxima = 2;

    private readonly WinForms.NotifyIcon _iconoBandeja;
    private readonly IHorarioClaseRepository _horarioRepository;
    private readonly INotificacionRepository _notificacionRepository;
    private readonly IConfiguracionNotificacionRepository _configuracionNotificacionRepository;
    private readonly IRecordatorioTareaRepository _recordatorioTareaRepository;
    private readonly ReproductorSonidoService _reproductorSonido = new();
    private readonly int _usuarioId;

    public ServicioNotificaciones(
        IHorarioClaseRepository horarioRepository,
        INotificacionRepository notificacionRepository,
        IConfiguracionNotificacionRepository configuracionNotificacionRepository,
        IRecordatorioTareaRepository recordatorioTareaRepository,
        int usuarioId)
    {
        _horarioRepository = horarioRepository;
        _notificacionRepository = notificacionRepository;
        _configuracionNotificacionRepository = configuracionNotificacionRepository;
        _recordatorioTareaRepository = recordatorioTareaRepository;
        _usuarioId = usuarioId;

        _iconoBandeja = new WinForms.NotifyIcon
        {
            Icon = System.Drawing.SystemIcons.Application, // temporal: icono propio pendiente de diseño
            Visible = true,
            Text = "MARC TIME"
        };

        var menu = new WinForms.ContextMenuStrip();
        menu.Items.Add("Salir", null, (_, _) => Application.Current.Shutdown());
        _iconoBandeja.ContextMenuStrip = menu;
    }

    public void RevisarYNotificar()
    {
        RevisarHorariosProximos();
        RevisarRecordatoriosTarea();
    }

    private void RevisarHorariosProximos()
    {
        var horarios = _horarioRepository.ObtenerProximasHoy(_usuarioId, MinutosAntelacionClase);

        foreach (var horario in horarios)
        {
            if (_notificacionRepository.YaSeNotificoHoy(_usuarioId, TipoEventoClaseProxima, tareaId: null, horarioClaseId: horario.HorarioClaseId))
            {
                continue;
            }

            string mensaje = $"Tu clase empieza a las {horario.HoraInicio:hh\\:mm}" +
                (string.IsNullOrWhiteSpace(horario.Ubicacion) ? "." : $" en {horario.Ubicacion}.");

            MostrarGlobo("Clase próxima", mensaje, TipoEventoClaseProxima);

            _notificacionRepository.Registrar(new Notificacion
            {
                UsuarioId = _usuarioId,
                TipoEventoId = TipoEventoClaseProxima,
                Mensaje = mensaje,
                HorarioClaseId = horario.HorarioClaseId
            });
        }
    }

    private void RevisarRecordatoriosTarea()
    {
        var pendientes = _recordatorioTareaRepository.ObtenerPendientes(_usuarioId);

        foreach (var recordatorio in pendientes)
        {
            string mensaje = recordatorio.MinutosAntelacion >= 1440
                ? $"\"{recordatorio.Titulo}\" vence en {recordatorio.MinutosAntelacion / 1440} dia(s), el {recordatorio.FechaEntrega:dd/MM HH:mm}."
                : recordatorio.MinutosAntelacion >= 60
                    ? $"\"{recordatorio.Titulo}\" vence en {recordatorio.MinutosAntelacion / 60} hora(s)."
                    : $"\"{recordatorio.Titulo}\" vence en {recordatorio.MinutosAntelacion} minuto(s).";

            MostrarGlobo("Recordatorio de tarea", mensaje, TipoEventoTareaProxima);

            _recordatorioTareaRepository.MarcarEnviado(recordatorio.RecordatorioTareaId);

            _notificacionRepository.Registrar(new Notificacion
            {
                UsuarioId = _usuarioId,
                TipoEventoId = TipoEventoTareaProxima,
                Mensaje = mensaje,
                TareaId = recordatorio.TareaId
            });
        }
    }

    private void MostrarGlobo(string titulo, string mensaje, int tipoEventoId)
    {
        Debug.WriteLine($"NOTIFICACION -> [{titulo}] {mensaje}");
        _iconoBandeja.BalloonTipTitle = titulo;
        _iconoBandeja.BalloonTipText = mensaje;
        _iconoBandeja.ShowBalloonTip(5000);

        var sonido = _configuracionNotificacionRepository.ResolverSonido(_usuarioId, tipoEventoId);
        if (sonido.Activo)
        {
            _reproductorSonido.Reproducir(sonido.RutaArchivo);
        }
    }

    public void Dispose()
    {
        _iconoBandeja.Visible = false;
        _iconoBandeja.Dispose();
    }
}