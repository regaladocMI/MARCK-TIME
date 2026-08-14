using Dapper;
using MarcTime.Core.Models.Academico;
using MarcTime.Data.Conexion;

namespace MarcTime.Data.Repositories;

/// <summary>
/// CRUD sobre Cursos. Sin RowVersion (esta tabla no lo tiene, a diferencia
/// de Tareas/Aplicaciones - se edita con poca frecuencia).
///
/// NOTA sobre Eliminar(): Cursos->HorariosClase es CASCADE (se borran solos),
/// pero Cursos->Tareas es NO ACTION (decision de la Seccion 2, para evitar el
/// error 1785 de multiples rutas de cascada). Si el curso tiene tareas
/// asociadas, SQL Server rechaza el DELETE. La UI (seccion futura) debe
/// capturar ese caso y avisar al usuario en vez de dejar que reviente.
/// </summary>
public class CursoRepository : ICursoRepository
{
    private readonly IConexionFactory _fabricaConexion;

    public CursoRepository(IConexionFactory fabricaConexion)
    {
        _fabricaConexion = fabricaConexion;
    }

    public int Crear(Curso curso)
    {
        const string sql = """
            INSERT INTO Cursos (UsuarioId, PeriodoId, Nombre, Codigo, Color)
            VALUES (@UsuarioId, @PeriodoId, @Nombre, @Codigo, @Color);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;

        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.ExecuteScalar<int>(sql, curso);
    }

    public Curso? ObtenerPorId(int cursoId)
    {
        const string sql = "SELECT * FROM Cursos WHERE CursoId = @CursoId;";
        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.QuerySingleOrDefault<Curso>(sql, new { CursoId = cursoId });
    }

    public List<Curso> ObtenerTodos(int usuarioId)
    {
        const string sql = "SELECT * FROM Cursos WHERE UsuarioId = @UsuarioId ORDER BY Nombre;";
        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.Query<Curso>(sql, new { UsuarioId = usuarioId }).ToList();
    }

    public bool Actualizar(Curso curso)
    {
        const string sql = """
            UPDATE Cursos
            SET PeriodoId = @PeriodoId,
                Nombre = @Nombre,
                Codigo = @Codigo,
                Color = @Color
            WHERE CursoId = @CursoId;
            """;

        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.Execute(sql, curso) > 0;
    }

    public bool Eliminar(int cursoId)
    {
        const string sql = "DELETE FROM Cursos WHERE CursoId = @CursoId;";
        using var conexion = _fabricaConexion.CrearConexionAbierta();
        return conexion.Execute(sql, new { CursoId = cursoId }) > 0;
    }
}