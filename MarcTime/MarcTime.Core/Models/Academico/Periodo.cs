namespace MarcTime.Core.Models.Academico;

/// <summary>
/// Refleja Periodos. Opcional a proposito: un usuario que no maneja
/// ciclos/semestres simplemente nunca crea filas aqui.
/// </summary>
public class Periodo
{
    public int PeriodoId { get; set; }
    public int UsuarioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public bool Activo { get; set; } = true;
}