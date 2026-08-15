using System.Diagnostics;
using System.Windows;
using MarcTime.Core.Models.Notificaciones;
using MarcTime.Data.Repositories;
using WinForms = System.Windows.Forms;

namespace MarcTime.UI.Servicios;

/// <summary>
/// Dueño del icono de bandeja del sistema. Revisa periodicamente horarios
/// proximos y tareas proximas a vencer, y muestra un globo (balloon tip) por
/// cada evento nuevo - registrando primero en Notificaciones para no repetir
/// el mismo aviso el mismo dia (YaSeNotificoHoy).
///
/// IDs de TiposEvento esperados en la BD (ver seed de datos, Seccion 2):
///   1 = ClaseProxima, 2 = TareaProxima
/// </summary>
public class ServicioNotificaciones : IDisposable
{
    private const int MinutosAntelacionClase = 15;
    private const int HorasAntelacionTarea = 24;
    private const int TipoEventoClaseProxima = 1;
    private const int TipoEventoTareaProxima = 2;

    private readonly WinForms.NotifyIcon _iconoBandeja;
    private readonly IHorarioClaseRepository _horarioRepository;
    private readonly ITareaRepository _tareaRepository;
    private readonly INotificacionRepository _notificacionRepository;
    private readonly int _usuarioId;

    public ServicioNotificaciones(
        IHorarioClaseRepository horarioRepository,
        ITareaRepository tareaRepository,
        INotificacionRepository notificacionRepository,
        int usuarioId)
    {
        _horarioRepository = horarioRepository;
        _tareaRepository = tareaRepository;
        _notificacionRepository = notificacionRepository;
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
        RevisarTareasProximas();
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

            MostrarGlobo("Clase próxima", mensaje);

            _notificacionRepository.Registrar(new Notificacion
            {
                UsuarioId = _usuarioId,
                TipoEventoId = TipoEventoClaseProxima,
                Mensaje = mensaje,
                HorarioClaseId = horario.HorarioClaseId
            });
        }
    }

    private void RevisarTareasProximas()
    {
        var tareas = _tareaRepository.ObtenerProximasAVencer(_usuarioId, diasAntelacion: 1);

        foreach (var tarea in tareas)
        {
            if (_notificacionRepository.YaSeNotificoHoy(_usuarioId, TipoEventoTareaProxima, tareaId: tarea.TareaId, horarioClaseId: null))
            {
                continue;
            }

            string mensaje = $"\"{tarea.Titulo}\" vence el {tarea.FechaEntrega:dd/MM HH:mm}.";

            MostrarGlobo("Tarea próxima a vencer", mensaje);

            _notificacionRepository.Registrar(new Notificacion
            {
                UsuarioId = _usuarioId,
                TipoEventoId = TipoEventoTareaProxima,
                Mensaje = mensaje,
                TareaId = tarea.TareaId
            });
        }
    }

    private void MostrarGlobo(string titulo, string mensaje)
    {
        Debug.WriteLine($"NOTIFICACION -> [{titulo}] {mensaje}");
        _iconoBandeja.BalloonTipTitle = titulo;
        _iconoBandeja.BalloonTipText = mensaje;
        _iconoBandeja.ShowBalloonTip(5000);
    }

    public void Dispose()
    {
        _iconoBandeja.Visible = false;
        _iconoBandeja.Dispose();
    }
}