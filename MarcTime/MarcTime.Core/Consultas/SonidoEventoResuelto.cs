namespace MarcTime.Core.Consultas;

/// <summary>
/// Resultado de resolver que sonido usar para un usuario+tipo de evento:
/// el que el usuario configuro, o el predeterminado del sistema si no
/// configuro nada todavia. No refleja una tabla 1 a 1.
/// </summary>
public class SonidoEventoResuelto
{
    public string? RutaArchivo { get; set; }
    public bool Activo { get; set; } = true;
}