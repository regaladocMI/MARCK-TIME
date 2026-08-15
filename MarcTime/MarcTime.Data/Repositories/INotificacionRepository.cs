using MarcTime.Core.Models.Notificaciones;

namespace MarcTime.Data.Repositories;

public interface INotificacionRepository
{
    long Registrar(Notificacion notificacion);
    bool YaSeNotificoHoy(int usuarioId, int tipoEventoId, int? tareaId, int? horarioClaseId);
}