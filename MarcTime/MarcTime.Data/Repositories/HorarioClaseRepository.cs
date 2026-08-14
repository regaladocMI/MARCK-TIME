using Dapper;
using MarcTime.Core.Models.Academico;
using MarcTime.Data.Conexion;

namespace MarcTime.Data.Repositories;

/// <summary>
/// CRUD sobre HorariosClase. ObtenerTodosPorUsuario hace JOIN con Cursos
/// porque HorariosClase no tiene UsuarioId propio (llega a el via CursoId).
/// </summary>
public class HorarioClaseRepository : IHorarioClaseRepository
{
    private readonly IConexionFactory _fabricaConexion;

    public HorarioClaseRepository(IConexionFactory fabricaConexion)
    {
        _fabricaConexion = fabricaConexion;
    }

    public int Crear(HorarioClase horario)
    {
        const string sql = """
            INSERT INTO HorariosClase (CursoId, DiaSemana, HoraInicio, HoraFin, Ubicacion)
            VALUES (@CursoId, @DiaSemana, @HoraInicio, @HoraFin, @Ubicacion);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;

        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.ExecuteScalar<int>(sql, horario);
    }

    public HorarioClase? ObtenerPorId(int horarioClaseId)
    {
        const string sql = "SELECT * FROM HorariosClase WHERE HorarioClaseId = @HorarioClaseId;";
        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.QuerySingleOrDefault<HorarioClase>(sql, new { HorarioClaseId = horarioClaseId });
    }

    public List<HorarioClase> ObtenerPorCurso(int cursoId)
    {
        const string sql = """
            SELECT * FROM HorariosClase
            WHERE CursoId = @CursoId
            ORDER BY DiaSemana, HoraInicio;
            """;
        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.Query<HorarioClase>(sql, new { CursoId = cursoId }).ToList();
    }

    public List<HorarioClase> ObtenerTodosPorUsuario(int usuarioId)
    {
        const string sql = """
            SELECT h.* FROM HorariosClase h
            INNER JOIN Cursos c ON c.CursoId = h.CursoId
            WHERE c.UsuarioId = @UsuarioId
            ORDER BY h.DiaSemana, h.HoraInicio;
            """;
        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.Query<HorarioClase>(sql, new { UsuarioId = usuarioId }).ToList();
    }

    public bool Actualizar(HorarioClase horario)
    {
        const string sql = """
            UPDATE HorariosClase
            SET DiaSemana = @DiaSemana,
                HoraInicio = @HoraInicio,
                HoraFin = @HoraFin,
                Ubicacion = @Ubicacion
            WHERE HorarioClaseId = @HorarioClaseId;
            """;

        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.Execute(sql, horario) > 0;
    }

    public bool Eliminar(int horarioClaseId)
    {
        const string sql = "DELETE FROM HorariosClase WHERE HorarioClaseId = @HorarioClaseId;";
        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.Execute(sql, new { HorarioClaseId = horarioClaseId }) > 0;
    }
}