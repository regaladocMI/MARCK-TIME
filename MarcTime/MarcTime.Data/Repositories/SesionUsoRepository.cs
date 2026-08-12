using Dapper;
using MarcTime.Core.Models.Monitoreo;
using MarcTime.Data.Conexion;

namespace MarcTime.Data.Repositories;

/// <summary>
/// Maneja la apertura/cierre de sesiones de uso. No calcula minutos ni
/// escribe en ResumenUsoDiario: eso lo hace automaticamente el trigger de
/// SQL Server (trg_SesionesUso_ActualizarResumen) cuando CerrarSesion hace
/// el UPDATE que fija FechaHoraFin.
///
/// NOTA: SesionesUso tiene un trigger AFTER INSERT/UPDATE. SQL Server no
/// permite "OUTPUT INSERTED.columna" directo al cliente en tablas con
/// triggers (solo con OUTPUT ... INTO una tabla temporal). Por eso aqui se
/// usa SCOPE_IDENTITY() en vez de OUTPUT para obtener el Id generado.
/// </summary>
public class SesionUsoRepository : ISesionUsoRepository
{
    private readonly IConexionFactory _fabricaConexion;

    public SesionUsoRepository(IConexionFactory fabricaConexion)
    {
        _fabricaConexion = fabricaConexion;
    }

    public long AbrirSesion(int aplicacionId, DateTime fechaHoraInicio, string? tituloVentana)
    {
        const string sql = """
            INSERT INTO SesionesUso (AplicacionId, FechaHoraInicio, TituloVentana)
            VALUES (@AplicacionId, @FechaHoraInicio, @TituloVentana);
            SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
            """;

        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.ExecuteScalar<long>(sql, new { AplicacionId = aplicacionId, FechaHoraInicio = fechaHoraInicio, TituloVentana = tituloVentana });
    }

    public bool CerrarSesion(long sesionUsoId, DateTime fechaHoraFin)
    {
        const string sql = """
            UPDATE SesionesUso
            SET FechaHoraFin = @FechaHoraFin
            WHERE SesionUsoId = @SesionUsoId
              AND FechaHoraFin IS NULL;
            """;

        using var conexion = _fabricaConexion.CrearConexionAbierta();
        int filasAfectadas = conexion.Execute(sql, new { SesionUsoId = sesionUsoId, FechaHoraFin = fechaHoraFin });
        return filasAfectadas > 0;
    }

    public SesionUso? ObtenerSesionAbierta(int aplicacionId)
    {
        const string sql = """
            SELECT * FROM SesionesUso
            WHERE AplicacionId = @AplicacionId AND FechaHoraFin IS NULL;
            """;

        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.QuerySingleOrDefault<SesionUso>(sql, new { AplicacionId = aplicacionId });
    }
}