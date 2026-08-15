using Dapper;
using MarcTime.Core.Consultas;
using MarcTime.Core.Models.Academico;
using MarcTime.Data.Conexion;

namespace MarcTime.Data.Repositories;

/// <summary>
/// CRUD sobre HorariosClase. ObtenerTodosPorUsuario hace JOIN con Cursos
/// porque HorariosClase no tiene UsuarioId propio (llega a el via CursoId).
/// </summary>
public class HorarioClaseRepository : IHorarioClaseRepository
{

    //Agregado 16
    public List<BloqueHorarioDetalle> ObtenerDetalladoPorUsuario(int usuarioId)
    {
        const string sql = """
            SELECT h.HorarioClaseId, c.CursoId, c.Nombre AS NombreCurso, c.Color,
                   h.DiaSemana, h.HoraInicio, h.HoraFin, h.Ubicacion
            FROM HorariosClase h
            INNER JOIN Cursos c ON c.CursoId = h.CursoId
            WHERE c.UsuarioId = @UsuarioId
            ORDER BY h.DiaSemana, h.HoraInicio;
            """;
        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.Query<Core.Consultas.BloqueHorarioDetalle>(sql, new { UsuarioId = usuarioId }).ToList();
    }

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
            SET CursoId = @CursoId,
                DiaSemana = @DiaSemana,
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



    //AGREGADO 11
    public List<HorarioClase> ObtenerProximasHoy(int usuarioId, int minutosAntelacion)
    {
        // DiaSemana en la BD: 1=Lunes...7=Domingo. DATEPART(WEEKDAY,...) de SQL
        // Server depende del DATEFIRST de la sesion; se normaliza con modulo.
        const string sql = """
            DECLARE @DiaHoy TINYINT = ((DATEPART(WEEKDAY, SYSDATETIME()) + @@DATEFIRST - 2) % 7) + 1;
            DECLARE @HoraActual TIME = CAST(SYSDATETIME() AS TIME);

            SELECT h.* FROM HorariosClase h
            INNER JOIN Cursos c ON c.CursoId = h.CursoId
            WHERE c.UsuarioId = @UsuarioId
              AND h.DiaSemana = @DiaHoy
              AND h.HoraInicio BETWEEN @HoraActual AND DATEADD(MINUTE, @MinutosAntelacion, @HoraActual)
            ORDER BY h.HoraInicio;
            """;

        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.Query<HorarioClase>(sql, new { UsuarioId = usuarioId, MinutosAntelacion = minutosAntelacion }).ToList();
    }
}