namespace MarcTime.Core.Models.Monitoreo;

/// <summary>
/// Refleja CategoriasAplicacion. Catalogo global (no por usuario) para que
/// los reportes sean comparables entre usuarios en la futura version
/// multiusuario.
/// </summary>
public class CategoriaAplicacion
{
    public int CategoriaAplicacionId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool EsProductiva { get; set; }
}