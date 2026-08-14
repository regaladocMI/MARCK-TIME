using Dapper;
using MarcTime.Core.Models.Tareas;
using MarcTime.Data.Conexion;

namespace MarcTime.Data.Repositories;

/// <summary>
/// CRUD sobre Tareas. Actualizar() usa RowVersion en el WHERE (concurrencia
/// optimista, igual que AplicacionRepository.Actualizar en la Seccion 4).
/// MarcarCompletada() es un metodo aparte, sin RowVersion: pensado para el
/// clic rapido de un checkbox en la UI, no para edicion completa del formulario.
/// </summary>
public class TareaRepository : ITareaRepository
{
    private readonly IConexionFactory _fabricaConexion;

    public TareaRepository(IConexionFactory fabricaConexion)
    {
        _fabricaConexion = fabricaConexion;
    }

    public int Crear(Tarea tarea)
    {
        const string sql = """
            INSERT INTO Tareas (UsuarioId, CursoId, Titulo, Descripcion, FechaEntrega, Prioridad, Completada)
            OUTPUT INSERTED.TareaId
            VALUES (@UsuarioId, @CursoId, @Titulo, @Descripcion, @FechaEntrega, @Prioridad, @Completada);
            """;

        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.ExecuteScalar<int>(sql, tarea);
    }

    public Tarea? ObtenerPorId(int tareaId)
    {
        const string sql = "SELECT * FROM Tareas WHERE TareaId = @TareaId;";
        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.QuerySingleOrDefault<Tarea>(sql, new { TareaId = tareaId });
    }

    public List<Tarea> ObtenerTodas(int usuarioId)
    {
        const string sql = """
            SELECT * FROM Tareas
            WHERE UsuarioId = @UsuarioId
            ORDER BY FechaEntrega;
            """;
        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.Query<Tarea>(sql, new { UsuarioId = usuarioId }).ToList();
    }

    public List<Tarea> ObtenerPendientes(int usuarioId)
    {
        const string sql = """
            SELECT * FROM Tareas
            WHERE UsuarioId = @UsuarioId AND Completada = 0
            ORDER BY FechaEntrega;
            """;
        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.Query<Tarea>(sql, new { UsuarioId = usuarioId }).ToList();
    }

    public List<Tarea> ObtenerProximasAVencer(int usuarioId, int diasAntelacion)
    {
        // Usado por la Seccion 11 (notificaciones) para saber que tareas
        // avisar. Solo tareas pendientes, dentro de la ventana de dias.
        const string sql = """
            SELECT * FROM Tareas
            WHERE UsuarioId = @UsuarioId
              AND Completada = 0
              AND FechaEntrega BETWEEN SYSDATETIME() AND DATEADD(DAY, @DiasAntelacion, SYSDATETIME())
            ORDER BY FechaEntrega;
            """;
        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.Query<Tarea>(sql, new { UsuarioId = usuarioId, DiasAntelacion = diasAntelacion }).ToList();
    }

    public bool Actualizar(Tarea tarea)
    {
        const string sql = """
            UPDATE Tareas
            SET CursoId = @CursoId,
                Titulo = @Titulo,
                Descripcion = @Descripcion,
                FechaEntrega = @FechaEntrega,
                Prioridad = @Prioridad,
                Completada = @Completada,
                FechaActualizacion = SYSDATETIME()
            WHERE TareaId = @TareaId
              AND RowVersion = @RowVersion;
            """;

        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.Execute(sql, tarea) > 0;
    }

    public bool MarcarCompletada(int tareaId, bool completada)
    {
        const string sql = """
            UPDATE Tareas
            SET Completada = @Completada,
                FechaActualizacion = SYSDATETIME()
            WHERE TareaId = @TareaId;
            """;

        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.Execute(sql, new { TareaId = tareaId, Completada = completada }) > 0;
    }

    public bool Eliminar(int tareaId)
    {
        const string sql = "DELETE FROM Tareas WHERE TareaId = @TareaId;";
        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.Execute(sql, new { TareaId = tareaId }) > 0;
    }
}