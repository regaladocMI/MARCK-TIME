using MarcTime.Core.Models.Notificaciones;

namespace MarcTime.Data.Repositories;

public interface ISonidoRepository
{
    int Crear(Sonido sonido);
    List<Sonido> ObtenerDisponibles(int usuarioId);
}