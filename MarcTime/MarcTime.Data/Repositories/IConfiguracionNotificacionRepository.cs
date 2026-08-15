using MarcTime.Core.Consultas;
using MarcTime.Core.Models.Notificaciones;

namespace MarcTime.Data.Repositories;

public interface IConfiguracionNotificacionRepository
{
    SonidoEventoResuelto ResolverSonido(int usuarioId, int tipoEventoId);
    bool Establecer(ConfiguracionNotificacion configuracion);
}