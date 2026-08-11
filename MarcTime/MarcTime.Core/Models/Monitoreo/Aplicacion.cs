namespace MarcTime.Core.Models.Monitoreo;

/// <summary>
/// Refleja Aplicaciones: el catalogo de programas monitoreados por usuario.
/// </summary>
public class Aplicacion
{
    public int AplicacionId { get; set; }
    public int UsuarioId { get; set; }
    public int? CategoriaAplicacionId { get; set; }
    public string NombreEjecutable { get; set; } = string.Empty;
    public string NombreVisible { get; set; } = string.Empty;
    public int? LimiteMinutosDiarios { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public byte[]? RowVersion { get; set; }
}