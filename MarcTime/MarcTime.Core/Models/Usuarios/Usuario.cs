namespace MarcTime.Core.Models.Usuarios;

/// <summary>
/// Refleja la tabla Usuarios. Es la raiz de todo el modelo: todo lo demas
/// cuelga, directa o indirectamente, de un UsuarioId.
/// </summary>
public class Usuario
{
    public int UsuarioId { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string CorreoElectronico { get; set; } = string.Empty;
    public string HashContrasena { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public bool Activo { get; set; } = true;
}