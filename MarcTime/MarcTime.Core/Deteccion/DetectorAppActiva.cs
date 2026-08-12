using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace MarcTime.Core.Deteccion;

/// <summary>
/// Detecta la app en primer plano usando P/Invoke a user32.dll combinado
/// con Process. Solo funciona en Windows (las funciones nativas que llama
/// son propias de ese sistema operativo).
/// </summary>
public class DetectorAppActiva : IDetectorAppActiva
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder texto, int cantidadMaxima);

    [DllImport("user32.dll")]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    public AppActivaInfo? ObtenerAppActiva()
    {
        IntPtr handleVentana = GetForegroundWindow();
        if (handleVentana == IntPtr.Zero)
        {
            return null; // ninguna ventana en primer plano (caso raro, ej. escritorio vacio)
        }

        GetWindowThreadProcessId(handleVentana, out uint processId);
        if (processId == 0)
        {
            return null;
        }

        try
        {
            using Process proceso = Process.GetProcessById((int)processId);

            return new AppActivaInfo
            {
                ProcessId = (int)processId,
                NombreEjecutable = proceso.ProcessName + ".exe",
                TituloVentana = ObtenerTituloVentana(handleVentana)
            };
        }
        catch (ArgumentException)
        {
            // El proceso ya no existe: se cerro entre GetWindowThreadProcessId
            // y este punto. Es una condicion de carrera esperable, no un error.
            return null;
        }
        catch (InvalidOperationException)
        {
            // El proceso existe pero ya termino (estado inconsistente momentaneo).
            return null;
        }
    }

    private static string ObtenerTituloVentana(IntPtr handleVentana)
    {
        int longitud = GetWindowTextLength(handleVentana);
        if (longitud == 0)
        {
            return string.Empty;
        }

        var buffer = new StringBuilder(longitud + 1);
        GetWindowText(handleVentana, buffer, buffer.Capacity);
        return buffer.ToString();
    }
}