using Dapper;
using MarcTime.Core.Consultas;
using MarcTime.Data.Conexion;

namespace MarcTime.Data.Repositories;

/// <summary>
/// ObtenerAppsFueraDeHorario() es el corazon de la Seccion 14: para cada app
/// que tiene al menos una restriccion de horario asignada, revisa si la
/// hora actual cae DENTRO de alguno de sus bloques permitidos hoy. Si no
/// cae en ninguno, la app aparece en el resultado (hay que cerrarla).
/// </summary>
public class RestriccionHorarioAppRepository : IRestriccionHorarioAppRepository
{
    private readonly IConexionFactory _fabricaConexion;

    public RestriccionHorarioAppRepository(IConexionFactory fabricaConexion)
    {
        _fabricaConexion = fabricaConexion;
    }

    public int Asignar(int horarioClaseId, int aplicacionId)
    {
        const string sql = """
            IF NOT EXISTS (SELECT 1 FROM RestriccionesHorarioApp WHERE HorarioClaseId = @HorarioClaseId AND AplicacionId = @AplicacionId)
            BEGIN
                INSERT INTO RestriccionesHorarioApp (HorarioClaseId, AplicacionId)
                VALUES (@HorarioClaseId, @AplicacionId);
            END
            SELECT RestriccionHorarioAppId FROM RestriccionesHorarioApp
            WHERE HorarioClaseId = @HorarioClaseId AND AplicacionId = @AplicacionId;
            """;

        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.ExecuteScalar<int>(sql, new { HorarioClaseId = horarioClaseId, AplicacionId = aplicacionId });
    }

    public bool Quitar(int horarioClaseId, int aplicacionId)
    {
        const string sql = """
            DELETE FROM RestriccionesHorarioApp
            WHERE HorarioClaseId = @HorarioClaseId AND AplicacionId = @AplicacionId;
            """;

        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.Execute(sql, new { HorarioClaseId = horarioClaseId, AplicacionId = aplicacionId }) > 0;
    }

    public List<AppFueraDeHorario> ObtenerAppsFueraDeHorario(int usuarioId)
    {
        const string sql = """
            DECLARE @DiaHoy TINYINT = ((DATEPART(WEEKDAY, SYSDATETIME()) + @@DATEFIRST - 2) % 7) + 1;
            DECLARE @HoraActual TIME = CAST(SYSDATETIME() AS TIME);

            SELECT DISTINCT a.AplicacionId, a.NombreEjecutable, a.NombreVisible
            FROM Aplicaciones a
            INNER JOIN RestriccionesHorarioApp r ON r.AplicacionId = a.AplicacionId
            WHERE a.UsuarioId = @UsuarioId
              AND a.Activo = 1
              AND NOT EXISTS (
                  SELECT 1
                  FROM RestriccionesHorarioApp r2
                  INNER JOIN HorariosClase h ON h.HorarioClaseId = r2.HorarioClaseId
                  WHERE r2.AplicacionId = a.AplicacionId
                    AND h.DiaSemana = @DiaHoy
                    AND @HoraActual BETWEEN h.HoraInicio AND h.HoraFin
              );
            """;

        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.Query<AppFueraDeHorario>(sql, new { UsuarioId = usuarioId }).ToList();
    }
}