using Dapper;
using MarcTime.Core.Consultas;
using MarcTime.Core.Models.Monitoreo;
using MarcTime.Data.Conexion;

namespace MarcTime.Data.Repositories;

/// <summary>
/// CRUD sobre Aplicaciones usando Dapper. Actualizar() incluye RowVersion en
/// el WHERE: si otra persona/proceso modifico la fila entre que la leiste y
/// que la guardas, la actualizacion afecta 0 filas y el metodo devuelve
/// false (concurrencia optimista, en vez de pisar el cambio ajeno sin avisar).
/// </summary>
public class AplicacionRepository : IAplicacionRepository
{
    private readonly IConexionFactory _fabricaConexion;

    public AplicacionRepository(IConexionFactory fabricaConexion)
    {
        _fabricaConexion = fabricaConexion;
    }

    public int Crear(Aplicacion aplicacion)
    {
        const string sql = """
            INSERT INTO Aplicaciones (UsuarioId, CategoriaAplicacionId, NombreEjecutable, NombreVisible, LimiteMinutosDiarios, Activo)
            OUTPUT INSERTED.AplicacionId
            VALUES (@UsuarioId, @CategoriaAplicacionId, @NombreEjecutable, @NombreVisible, @LimiteMinutosDiarios, @Activo);
            """;

        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.ExecuteScalar<int>(sql, aplicacion);
    }

    public Aplicacion? ObtenerPorId(int aplicacionId)
    {
        const string sql = "SELECT * FROM Aplicaciones WHERE AplicacionId = @AplicacionId;";
        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.QuerySingleOrDefault<Aplicacion>(sql, new { AplicacionId = aplicacionId });
    }

    public Aplicacion? ObtenerPorNombreEjecutable(int usuarioId, string nombreEjecutable)
    {
        const string sql = """
            SELECT * FROM Aplicaciones
            WHERE UsuarioId = @UsuarioId AND NombreEjecutable = @NombreEjecutable;
            """;
        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.QuerySingleOrDefault<Aplicacion>(sql, new { UsuarioId = usuarioId, NombreEjecutable = nombreEjecutable });
    }

    public Aplicacion ObtenerOCrearPorNombreEjecutable(int usuarioId, string nombreEjecutable, string nombreVisible)
    {
        Aplicacion? existente = ObtenerPorNombreEjecutable(usuarioId, nombreEjecutable);
        if (existente is not null)
        {
            return existente;
        }

        int nuevoId = Crear(new Aplicacion
        {
            UsuarioId = usuarioId,
            NombreEjecutable = nombreEjecutable,
            NombreVisible = nombreVisible,
            Activo = true
        });

        return ObtenerPorId(nuevoId)!;
    }

    public List<Aplicacion> ObtenerTodas(int usuarioId)
    {
        const string sql = "SELECT * FROM Aplicaciones WHERE UsuarioId = @UsuarioId ORDER BY NombreVisible;";
        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.Query<Aplicacion>(sql, new { UsuarioId = usuarioId }).ToList();
    }

    public bool Actualizar(Aplicacion aplicacion)
    {
        const string sql = """
            UPDATE Aplicaciones
            SET CategoriaAplicacionId = @CategoriaAplicacionId,
                NombreEjecutable = @NombreEjecutable,
                NombreVisible = @NombreVisible,
                LimiteMinutosDiarios = @LimiteMinutosDiarios,
                Activo = @Activo,
                FechaActualizacion = SYSDATETIME()
            WHERE AplicacionId = @AplicacionId
              AND RowVersion = @RowVersion;
            """;

        using var conexion = _fabricaConexion.CrearConexionAbierta();
        int filasAfectadas = conexion.Execute(sql, aplicacion);
        return filasAfectadas > 0;
    }

    public bool Eliminar(int aplicacionId)
    {
        const string sql = "DELETE FROM Aplicaciones WHERE AplicacionId = @AplicacionId;";
        using var conexion = _fabricaConexion.CrearConexionAbierta();
        int filasAfectadas = conexion.Execute(sql, new { AplicacionId = aplicacionId });
        return filasAfectadas > 0;
    }

    public bool EstablecerLimiteMinutosDiarios(int aplicacionId, int? limiteMinutos)
    {
        const string sql = """
            UPDATE Aplicaciones
            SET LimiteMinutosDiarios = @LimiteMinutos,
                FechaActualizacion = SYSDATETIME()
            WHERE AplicacionId = @AplicacionId;
            """;

        using var conexion = _fabricaConexion.CrearConexionAbierta();
        int filasAfectadas = conexion.Execute(sql, new { AplicacionId = aplicacionId, LimiteMinutos = limiteMinutos });
        return filasAfectadas > 0;
    }

    public List<EstadoLimiteAplicacion> ObtenerEstadoLimites(int usuarioId)
    {
        const string sql = """
            SELECT
                a.AplicacionId,
                a.NombreEjecutable,
                a.NombreVisible,
                a.LimiteMinutosDiarios,
                ISNULL(cerradas.SegundosCerrados, 0) + ISNULL(activa.SegundosActiva, 0) AS SegundosUsadosHoy
            FROM Aplicaciones a
            OUTER APPLY (
                SELECT SUM(s.DuracionSegundos) AS SegundosCerrados
                FROM SesionesUso s
                WHERE s.AplicacionId = a.AplicacionId
                  AND s.FechaHoraFin IS NOT NULL
                  AND CAST(s.FechaHoraInicio AS DATE) = CAST(SYSDATETIME() AS DATE)
            ) cerradas
            OUTER APPLY (
                SELECT DATEDIFF(SECOND, s.FechaHoraInicio, SYSDATETIME()) AS SegundosActiva
                FROM SesionesUso s
                WHERE s.AplicacionId = a.AplicacionId AND s.FechaHoraFin IS NULL
            ) activa
            WHERE a.UsuarioId = @UsuarioId AND a.Activo = 1
            ORDER BY a.NombreVisible;
            """;

        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.Query<EstadoLimiteAplicacion>(sql, new { UsuarioId = usuarioId }).ToList();
    }
}