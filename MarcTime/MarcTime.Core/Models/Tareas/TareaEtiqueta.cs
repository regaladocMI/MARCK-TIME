namespace MarcTime.Core.Models.Tareas;

/// <summary>
/// Refleja la tabla puente TareasEtiquetas (relacion N a M entre Tarea y
/// Etiqueta). No tiene Id propio: la llave primaria es el par (TareaId, EtiquetaId).
/// </summary>
public class TareaEtiqueta
{
    public int TareaId { get; set; }
    public int EtiquetaId { get; set; }
}