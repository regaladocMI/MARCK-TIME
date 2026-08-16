using MarcTime.Core.Consultas;
using MarcTime.Core.Models.Tareas;

namespace MarcTime.Data.Repositories;

public interface IRecordatorioTareaRepository
{

    List<RecordatorioTarea> ObtenerPorTarea(int tareaId);
    bool Eliminar(long recordatorioTareaId);

    long Crear(int tareaId, int minutosAntelacion);
    List<long> CrearPredeterminados(int tareaId, DateTime fechaEntrega);
    List<RecordatorioPendiente> ObtenerPendientes(int usuarioId);
    bool MarcarEnviado(long recordatorioTareaId);
}