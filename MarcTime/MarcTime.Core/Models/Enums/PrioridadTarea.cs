namespace MarcTime.Core.Models.Enums;

/// <summary>
/// Prioridad de una tarea. Coincide con el CHECK constraint de Tareas.Prioridad.
/// </summary>
public enum PrioridadTarea : byte
{
    Baja = 1,
    Media = 2,
    Alta = 3
}