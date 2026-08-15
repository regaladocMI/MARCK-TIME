using Dapper;
using MarcTime.Core.Models.Notificaciones;
using MarcTime.Data.Conexion;

namespace MarcTime.Data.Repositories;

public class SonidoRepository : ISonidoRepository
{
    private readonly IConexionFactory _fabricaConexion;

    public SonidoRepository(IConexionFactory fabricaConexion)
    {
        _fabricaConexion = fabricaConexion;
    }

    public int Crear(Sonido sonido)
    {
        const string sql = """
            INSERT INTO Sonidos (UsuarioId, NombreArchivo, RutaArchivo, EsPredeterminado)
            VALUES (@UsuarioId, @NombreArchivo, @RutaArchivo, @EsPredeterminado);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;

        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.ExecuteScalar<int>(sql, sonido);
    }

    /// <summary>Sonidos del sistema (UsuarioId NULL) + los propios del usuario.</summary>
    public List<Sonido> ObtenerDisponibles(int usuarioId)
    {
        const string sql = """
            SELECT * FROM Sonidos
            WHERE UsuarioId IS NULL OR UsuarioId = @UsuarioId
            ORDER BY EsPredeterminado DESC, NombreArchivo;
            """;
        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.Query<Sonido>(sql, new { UsuarioId = usuarioId }).ToList();
    }
}