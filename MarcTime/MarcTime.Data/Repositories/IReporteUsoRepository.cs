using MarcTime.Core.Consultas;

namespace MarcTime.Data.Repositories;

public interface IReporteUsoRepository
{
    List<AppUsoResumen> ObtenerUsoPorApp(int usuarioId, DateOnly fechaInicio, DateOnly fechaFin);
    List<UsoPorDia> ObtenerUsoPorDia(int usuarioId, DateOnly fechaInicio, DateOnly fechaFin);
    ResumenProductividad ObtenerResumenProductividad(int usuarioId, DateOnly fechaInicio, DateOnly fechaFin);

    bool BorrarHistorialDia(int usuarioId, DateOnly fecha);
    List<UsoPorDia> ObtenerUsoPorDiaDeApp(int usuarioId, int aplicacionId, DateOnly fechaInicio, DateOnly fechaFin);
}