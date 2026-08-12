using MarcTime.Core.Models.Monitoreo;

namespace MarcTime.Data.Repositories;

public interface IAplicacionRepository
{
    int Crear(Aplicacion aplicacion);
    Aplicacion? ObtenerPorId(int aplicacionId);
    List<Aplicacion> ObtenerTodas(int usuarioId);
    bool Actualizar(Aplicacion aplicacion);
    bool Eliminar(int aplicacionId);
}