using MarcTime.Core.Models.Academico;

namespace MarcTime.Data.Repositories;

public interface IHorarioClaseRepository
{
    int Crear(HorarioClase horario);
    HorarioClase? ObtenerPorId(int horarioClaseId);
    List<HorarioClase> ObtenerPorCurso(int cursoId);
    List<HorarioClase> ObtenerTodosPorUsuario(int usuarioId);
    bool Actualizar(HorarioClase horario);
    bool Eliminar(int horarioClaseId);
}