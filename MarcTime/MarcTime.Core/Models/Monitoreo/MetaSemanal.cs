namespace MarcTime.Core.Models.Monitoreo;

/// <summary>
/// Refleja MetasSemanales. Debe apuntar a una CategoriaAplicacionId O a una
/// AplicacionId, nunca ambas ni ninguna (mismo CHECK que en la base de datos).
/// </summary>
public class MetaSemanal
{
    public int MetaSemanalId { get; set; }
    public int UsuarioId { get; set; }
    public int? CategoriaAplicacionId { get; set; }
    public int? AplicacionId { get; set; }
    public int MinutosObjetivo { get; set; }
    public DateOnly FechaInicioSemana { get; set; }
}