namespace MarcTime.Core.Models.Enums;

/// <summary>
/// Tipo de operacion registrada en LogAuditoria. Los nombres coinciden con
/// el texto que se guarda en la columna AccionRealizada (INSERT/UPDATE/DELETE).
/// </summary>
public enum AccionAuditoria
{
    Insert,
    Update,
    Delete
}