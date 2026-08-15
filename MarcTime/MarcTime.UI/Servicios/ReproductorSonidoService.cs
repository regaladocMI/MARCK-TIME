using System.Diagnostics;
using System.Media;

namespace MarcTime.UI.Servicios;

/// <summary>
/// Envuelve System.Media.SoundPlayer. Play() es asincrono (no bloquea el
/// hilo de la UI). Si el archivo no existe o no es un .wav valido, no
/// lanza excepcion hacia arriba - solo lo registra en el log, para que un
/// sonido roto no tumbe el resto de la notificacion.
/// </summary>
public class ReproductorSonidoService
{
    public void Reproducir(string? rutaArchivo)
    {
        if (string.IsNullOrWhiteSpace(rutaArchivo))
        {
            return;
        }

        try
        {
            using var reproductor = new SoundPlayer(rutaArchivo);
            reproductor.Play();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"No se pudo reproducir el sonido '{rutaArchivo}': {ex.Message}");
        }
    }
}