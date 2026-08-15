namespace MarcTime.Core.Deteccion;

/// <summary>
/// Snapshot de la aplicacion que esta en primer plano en el momento en que
/// se pidio la deteccion. TituloVentana se captura desde ahora como base
/// para una futura deteccion de sitio web especifico dentro del navegador
/// (leyendo el titulo de la pestana), aunque en la version base no se usa
/// para eso todavia.
/// </summary>
public class AppActivaInfo
{
    public int ProcessId { get; set; }
    public string NombreEjecutable { get; set; } = string.Empty;
    public string TituloVentana { get; set; } = string.Empty;

    public string NombreVisible { get; set; } = string.Empty;
}