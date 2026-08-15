using Dapper;
using MarcTime.Core.Models.Notificaciones;
using MarcTime.Data.Conexion;

namespace MarcTime.Data.Repositories;

/// <summary>
/// YaSeNotificoHoy() es la clave para no repetir el mismo aviso: antes de
/// mostrar un globo, se pregunta si ya existe un registro de HOY para esa
/// combinacion especifica (tipo de evento + tarea u horario relacionado).
/// </summary>
public class NotificacionRepository : INotificacionRepository
{
    private readonly IConexionFactory _fabricaConexion;

    public NotificacionRepository(IConexionFactory fabricaConexion)
    {
        _fabricaConexion = fabricaConexion;
    }

    public long Registrar(Notificacion notificacion)
    {
        const string sql = """
            INSERT INTO Notificaciones (UsuarioId, TipoEventoId, Mensaje, Leida, TareaId, HorarioClaseId, AplicacionId)
            VALUES (@UsuarioId, @TipoEventoId, @Mensaje, @Leida, @TareaId, @HorarioClaseId, @AplicacionId);
            SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
            """;

        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.ExecuteScalar<long>(sql, notificacion);
    }

    public bool YaSeNotificoHoy(int usuarioId, int tipoEventoId, int? tareaId, int? horarioClaseId)
    {
        const string sql = """
            SELECT COUNT(1) FROM Notificaciones
            WHERE UsuarioId = @UsuarioId
              AND TipoEventoId = @TipoEventoId
              AND ISNULL(TareaId, -1) = ISNULL(@TareaId, -1)
              AND ISNULL(HorarioClaseId, -1) = ISNULL(@HorarioClaseId, -1)
              AND CAST(FechaHoraEnvio AS DATE) = CAST(SYSDATETIME() AS DATE);
            """;

        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.ExecuteScalar<int>(sql, new { UsuarioId = usuarioId, TipoEventoId = tipoEventoId, TareaId = tareaId, HorarioClaseId = horarioClaseId }) > 0;
    }
}