namespace MarcTime.Core.Models.Enums;

/// <summary>
/// Dia de la semana para horarios de clase. Los valores coinciden con el
/// CHECK constraint de la tabla HorariosClase (1 = Lunes ... 7 = Domingo).
/// </summary>
public enum DiaSemana : byte
{
    Lunes = 1,
    Martes = 2,
    Miercoles = 3,
    Jueves = 4,
    Viernes = 5,
    Sabado = 6,
    Domingo = 7
}