using Dapper;
using MarcTime.Core.Consultas;
using MarcTime.Data.Conexion;

namespace MarcTime.Data.Repositories;

/// <summary>
/// Consultas de solo lectura sobre ResumenUsoDiario (tabla pre-agregada por
/// el trigger de la Seccion 2). fechaInicio == fechaFin sirve para un
/// reporte de un solo dia; un rango de 7 dias da el reporte semanal.
/// </summary>
public class ReporteUsoRepository : IReporteUsoRepository
{
    private readonly IConexionFactory _fabricaConexion;

    public ReporteUsoRepository(IConexionFactory fabricaConexion)
    {
        _fabricaConexion = fabricaConexion;
    }

    public List<AppUsoResumen> ObtenerUsoPorApp(int usuarioId, DateOnly fechaInicio, DateOnly fechaFin)
    {
        const string sql = """
            SELECT
                a.AplicacionId,
                a.NombreVisible,
                ca.Nombre AS NombreCategoria,
                ISNULL(ca.EsProductiva, 0) AS EsProductiva,
                SUM(r.MinutosTotales) AS MinutosTotales
            FROM ResumenUsoDiario r
            INNER JOIN Aplicaciones a ON a.AplicacionId = r.AplicacionId
            LEFT JOIN CategoriasAplicacion ca ON ca.CategoriaAplicacionId = a.CategoriaAplicacionId
            WHERE a.UsuarioId = @UsuarioId AND r.Fecha BETWEEN @FechaInicio AND @FechaFin
            GROUP BY a.AplicacionId, a.NombreVisible, ca.Nombre, ca.EsProductiva
            ORDER BY MinutosTotales DESC;
            """;

        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.Query<AppUsoResumen>(sql, new { UsuarioId = usuarioId, FechaInicio = fechaInicio, FechaFin = fechaFin }).ToList();
    }

    public List<UsoPorDia> ObtenerUsoPorDia(int usuarioId, DateOnly fechaInicio, DateOnly fechaFin)
    {
        const string sql = """
            SELECT r.Fecha, SUM(r.MinutosTotales) AS MinutosTotales
            FROM ResumenUsoDiario r
            INNER JOIN Aplicaciones a ON a.AplicacionId = r.AplicacionId
            WHERE a.UsuarioId = @UsuarioId AND r.Fecha BETWEEN @FechaInicio AND @FechaFin
            GROUP BY r.Fecha
            ORDER BY r.Fecha;
            """;

        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.Query<UsoPorDia>(sql, new { UsuarioId = usuarioId, FechaInicio = fechaInicio, FechaFin = fechaFin }).ToList();
    }

    public ResumenProductividad ObtenerResumenProductividad(int usuarioId, DateOnly fechaInicio, DateOnly fechaFin)
    {
        const string sql = """
            SELECT
                ISNULL(SUM(CASE WHEN ca.EsProductiva = 1 THEN r.MinutosTotales ELSE 0 END), 0) AS MinutosProductivos,
                ISNULL(SUM(CASE WHEN ca.CategoriaAplicacionId IS NOT NULL AND ca.EsProductiva = 0 THEN r.MinutosTotales ELSE 0 END), 0) AS MinutosNoProductivos,
                ISNULL(SUM(CASE WHEN ca.CategoriaAplicacionId IS NULL THEN r.MinutosTotales ELSE 0 END), 0) AS MinutosSinCategoria
            FROM ResumenUsoDiario r
            INNER JOIN Aplicaciones a ON a.AplicacionId = r.AplicacionId
            LEFT JOIN CategoriasAplicacion ca ON ca.CategoriaAplicacionId = a.CategoriaAplicacionId
            WHERE a.UsuarioId = @UsuarioId AND r.Fecha BETWEEN @FechaInicio AND @FechaFin;
            """;

        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.QuerySingleOrDefault<ResumenProductividad>(sql, new { UsuarioId = usuarioId, FechaInicio = fechaInicio, FechaFin = fechaFin })
            ?? new ResumenProductividad();
    }
}