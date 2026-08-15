using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace MarcTime.Core.Deteccion;

/// <summary>
/// Detecta la app en primer plano usando P/Invoke a user32.dll combinado
/// con Process. Solo funciona en Windows.
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
            return null;
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
                NombreVisible = ObtenerNombreVisible(proceso),
                TituloVentana = ObtenerTituloVentana(handleVentana)
            };
        }
        catch (ArgumentException)
        {
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private static string ObtenerNombreVisible(Process proceso)
    {
        try
        {
            string? descripcion = proceso.MainModule?.FileVersionInfo.FileDescription;
            if (!string.IsNullOrWhiteSpace(descripcion))
            {
                return descripcion;
            }
        }
        catch
        {
        }

        return proceso.ProcessName;
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