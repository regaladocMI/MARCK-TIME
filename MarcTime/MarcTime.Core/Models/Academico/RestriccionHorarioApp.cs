namespace MarcTime.Core.Models.Academico;

/// <summary>
/// Refleja RestriccionesHorarioApp: liga una app a un bloque de horario en
/// el que se le permite estar abierta. Una app puede tener varias filas
/// (varios bloques permitidos); si tiene al menos una, pasa a estar
/// "vigilada" - fuera de esos bloques, se cierra.
/// </summary>
public class RestriccionHorarioApp
{
    public int RestriccionHorarioAppId { get; set; }
    public int HorarioClaseId { get; set; }
    public int AplicacionId { get; set; }
}