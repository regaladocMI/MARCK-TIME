using MarcTime.Core.Consultas;

namespace MarcTime.Data.Repositories;

public interface IRestriccionHorarioAppRepository
{
    int Asignar(int horarioClaseId, int aplicacionId);
    bool Quitar(int horarioClaseId, int aplicacionId);
    List<AppFueraDeHorario> ObtenerAppsFueraDeHorario(int usuarioId);
    List<int> ObtenerAplicacionIdsPorHorario(int horarioClaseId);
}