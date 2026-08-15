using Dapper;
using MarcTime.Core.Consultas;
using MarcTime.Core.Models.Notificaciones;
using MarcTime.Data.Conexion;

namespace MarcTime.Data.Repositories;

/// <summary>
/// ResolverSonido() implementa la cascada de respaldo: sonido que el usuario
/// configuro para este tipo de evento -> si no configuro nada, el sonido
/// predeterminado del sistema (Sonidos.EsPredeterminado = 1).
/// </summary>
public class ConfiguracionNotificacionRepository : IConfiguracionNotificacionRepository
{
    private readonly IConexionFactory _fabricaConexion;

    public ConfiguracionNotificacionRepository(IConexionFactory fabricaConexion)
    {
        _fabricaConexion = fabricaConexion;
    }

    public SonidoEventoResuelto ResolverSonido(int usuarioId, int tipoEventoId)
    {
        const string sql = """
            SELECT
                ISNULL(sUsuario.RutaArchivo, sPredeterminado.RutaArchivo) AS RutaArchivo,
                ISNULL(cn.Activo, 1) AS Activo
            FROM TiposEvento te
            LEFT JOIN ConfiguracionNotificaciones cn
                ON cn.TipoEventoId = te.TipoEventoId AND cn.UsuarioId = @UsuarioId
            LEFT JOIN Sonidos sUsuario ON sUsuario.SonidoId = cn.SonidoId
            OUTER APPLY (
                SELECT TOP 1 RutaArchivo FROM Sonidos
                WHERE EsPredeterminado = 1
                ORDER BY SonidoId
            ) sPredeterminado
            WHERE te.TipoEventoId = @TipoEventoId;
            """;

        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.QuerySingleOrDefault<SonidoEventoResuelto>(sql, new { UsuarioId = usuarioId, TipoEventoId = tipoEventoId })
            ?? new SonidoEventoResuelto { RutaArchivo = null, Activo = true };
    }

    /// <summary>Crea o actualiza la preferencia de sonido/antelacion del usuario para un tipo de evento.</summary>
    public bool Establecer(ConfiguracionNotificacion configuracion)
    {
        const string sql = """
            MERGE ConfiguracionNotificaciones AS destino
            USING (SELECT @UsuarioId AS UsuarioId, @TipoEventoId AS TipoEventoId) AS origen
                ON destino.UsuarioId = origen.UsuarioId AND destino.TipoEventoId = origen.TipoEventoId
            WHEN MATCHED THEN
                UPDATE SET SonidoId = @SonidoId, MinutosAntelacion = @MinutosAntelacion, Activo = @Activo
            WHEN NOT MATCHED THEN
                INSERT (UsuarioId, TipoEventoId, SonidoId, MinutosAntelacion, Activo)
                VALUES (@UsuarioId, @TipoEventoId, @SonidoId, @MinutosAntelacion, @Activo);
            """;

        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.Execute(sql, configuracion) > 0;
    }
}