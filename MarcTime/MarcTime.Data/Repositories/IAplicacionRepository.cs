using MarcTime.Core.Models.Monitoreo;

namespace MarcTime.Data.Repositories;

public interface IAplicacionRepository
{
    int Crear(Aplicacion aplicacion);
    Aplicacion? ObtenerPorId(int aplicacionId);
    Aplicacion? ObtenerPorNombreEjecutable(int usuarioId, string nombreEjecutable);
    Aplicacion ObtenerOCrearPorNombreEjecutable(int usuarioId, string nombreEjecutable, string nombreVisible);
    List<Aplicacion> ObtenerTodas(int usuarioId);
    bool Actualizar(Aplicacion aplicacion);
    bool Eliminar(int aplicacionId);
}