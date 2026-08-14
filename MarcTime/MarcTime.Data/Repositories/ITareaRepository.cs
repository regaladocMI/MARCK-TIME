using MarcTime.Core.Models.Tareas;

namespace MarcTime.Data.Repositories;

public interface ITareaRepository
{
    int Crear(Tarea tarea);
    Tarea? ObtenerPorId(int tareaId);
    List<Tarea> ObtenerTodas(int usuarioId);
    List<Tarea> ObtenerPendientes(int usuarioId);
    List<Tarea> ObtenerProximasAVencer(int usuarioId, int diasAntelacion);
    bool Actualizar(Tarea tarea);
    bool MarcarCompletada(int tareaId, bool completada);
    bool Eliminar(int tareaId);
}