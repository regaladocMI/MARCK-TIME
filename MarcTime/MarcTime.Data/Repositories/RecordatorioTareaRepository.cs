using Dapper;
using MarcTime.Core.Consultas;
using MarcTime.Core.Models.Tareas;
using MarcTime.Data.Conexion;

namespace MarcTime.Data.Repositories;

/// <summary>
/// CrearPredeterminados() da un punto de partida razonable (3 dias, 1 dia,
/// 2 horas antes) sin obligar al usuario a configurar cada recordatorio a
/// mano; solo crea los que todavia caen en el futuro respecto a ahora.
/// </summary>
public class RecordatorioTareaRepository : IRecordatorioTareaRepository
{

    public List<RecordatorioTarea> ObtenerPorTarea(int tareaId)
    {
        const string sql = "SELECT * FROM RecordatoriosTarea WHERE TareaId = @TareaId ORDER BY MinutosAntelacion DESC;";
        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.Query<Core.Models.Tareas.RecordatorioTarea>(sql, new { TareaId = tareaId }).ToList();
    }

    public bool Eliminar(long recordatorioTareaId)
    {
        const string sql = "DELETE FROM RecordatoriosTarea WHERE RecordatorioTareaId = @Id;";
        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.Execute(sql, new { Id = recordatorioTareaId }) > 0;
    }

    private static readonly int[] OffsetsPredeterminadosMinutos = { 4320, 1440, 120 }; // 3d, 1d, 2h

    private readonly IConexionFactory _fabricaConexion;

    public RecordatorioTareaRepository(IConexionFactory fabricaConexion)
    {
        _fabricaConexion = fabricaConexion;
    }

    public long Crear(int tareaId, int minutosAntelacion)
    {
        const string sql = """
            INSERT INTO RecordatoriosTarea (TareaId, MinutosAntelacion)
            VALUES (@TareaId, @MinutosAntelacion);
            SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
            """;

        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.ExecuteScalar<long>(sql, new { TareaId = tareaId, MinutosAntelacion = minutosAntelacion });
    }

    public List<long> CrearPredeterminados(int tareaId, DateTime fechaEntrega)
    {
        var idsCreados = new List<long>();
        DateTime ahora = DateTime.Now;

        foreach (int offset in OffsetsPredeterminadosMinutos)
        {
            if (fechaEntrega.AddMinutes(-offset) > ahora)
            {
                idsCreados.Add(Crear(tareaId, offset));
            }
        }

        return idsCreados;
    }

    public List<RecordatorioPendiente> ObtenerPendientes(int usuarioId)
    {
        const string sql = """
            SELECT
                rt.RecordatorioTareaId,
                t.TareaId,
                t.Titulo,
                t.FechaEntrega,
                rt.MinutosAntelacion
            FROM RecordatoriosTarea rt
            INNER JOIN Tareas t ON t.TareaId = rt.TareaId
            WHERE t.UsuarioId = @UsuarioId
              AND t.Completada = 0
              AND rt.Enviado = 0
              AND DATEADD(MINUTE, -rt.MinutosAntelacion, t.FechaEntrega) <= SYSDATETIME();
            """;

        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.Query<RecordatorioPendiente>(sql, new { UsuarioId = usuarioId }).ToList();
    }

    public bool MarcarEnviado(long recordatorioTareaId)
    {
        const string sql = "UPDATE RecordatoriosTarea SET Enviado = 1 WHERE RecordatorioTareaId = @Id;";
        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.Execute(sql, new { Id = recordatorioTareaId }) > 0;
    }
}