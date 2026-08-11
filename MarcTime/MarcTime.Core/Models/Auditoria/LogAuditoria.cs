using MarcTime.Core.Models.Enums;

namespace MarcTime.Core.Models.Auditoria;

/// <summary>
/// Refleja LogAuditoria: rastro de cambios sensibles para trazabilidad.
/// </summary>
public class LogAuditoria
{
    public long LogAuditoriaId { get; set; }
    public int UsuarioId { get; set; }
    public string TablaAfectada { get; set; } = string.Empty;
    public AccionAuditoria AccionRealizada { get; set; }
    public string? ValorAnterior { get; set; }
    public string? ValorNuevo { get; set; }
    public DateTime FechaHora { get; set; }
}