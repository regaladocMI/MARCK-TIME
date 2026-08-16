namespace MarcTime.UI.ViewModels;

/// <summary>
/// Un recordatorio en edicion dentro del formulario de tarea. RecordatorioTareaId
/// es null si es nuevo (todavia no existe en la BD) - se usa para saber, al
/// guardar, cuales hay que Crear() y cuales ya existian.
/// </summary>
public class RecordatorioEditableViewModel
{
    public RecordatorioEditableViewModel(int minutosAntelacion, long? recordatorioTareaId = null)
    {
        MinutosAntelacion = minutosAntelacion;
        RecordatorioTareaId = recordatorioTareaId;
        Descripcion = Formatear(minutosAntelacion);
    }

    public long? RecordatorioTareaId { get; }
    public int MinutosAntelacion { get; }
    public string Descripcion { get; }

    private static string Formatear(int minutos)
    {
        if (minutos % 1440 == 0) return $"{minutos / 1440} día(s) antes";
        if (minutos % 60 == 0) return $"{minutos / 60} hora(s) antes";
        return $"{minutos} minuto(s) antes";
    }
}