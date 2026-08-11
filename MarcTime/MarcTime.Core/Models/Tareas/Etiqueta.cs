namespace MarcTime.Core.Models.Tareas;

/// <summary>
/// Refleja Etiquetas. Clasificacion libre definida por el usuario.
/// </summary>
public class Etiqueta
{
    public int EtiquetaId { get; set; }
    public int UsuarioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Color { get; set; }
}