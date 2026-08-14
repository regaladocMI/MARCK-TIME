using MarcTime.Core.Models.Academico;

namespace MarcTime.Data.Repositories;

public interface ICursoRepository
{
    int Crear(Curso curso);
    Curso? ObtenerPorId(int cursoId);
    List<Curso> ObtenerTodos(int usuarioId);
    bool Actualizar(Curso curso);
    bool Eliminar(int cursoId);
}