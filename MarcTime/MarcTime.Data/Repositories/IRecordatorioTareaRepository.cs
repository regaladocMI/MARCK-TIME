using MarcTime.Core.Consultas;

namespace MarcTime.Data.Repositories;

public interface IRecordatorioTareaRepository
{
    long Crear(int tareaId, int minutosAntelacion);
    List<long> CrearPredeterminados(int tareaId, DateTime fechaEntrega);
    List<RecordatorioPendiente> ObtenerPendientes(int usuarioId);
    bool MarcarEnviado(long recordatorioTareaId);
}