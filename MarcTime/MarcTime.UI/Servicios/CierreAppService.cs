using System.Diagnostics;
using System.Windows.Threading;

namespace MarcTime.UI.Servicios;

/// <summary>
/// Logica compartida de cierre de procesos: cierre amable (CloseMainWindow)
/// primero, cierre forzado (Kill) tras un periodo de gracia si el proceso
/// sigue vivo. La usan tanto GestorLimitesTiempoService (Seccion 8) como
/// GestorRestriccionesHorarioService (Seccion 14) para no duplicar esta logica.
/// </summary>
public static class CierreAppService
{
    private static readonly TimeSpan PeriodoGracia = TimeSpan.FromSeconds(15);

    /// <summary>Verifica si hay al menos un proceso corriendo con ese nombre - para no avisar/cerrar una app que ni siquiera esta abierta.</summary>
    public static bool EstaCorriendo(string nombreEjecutable) => ObtenerProcesos(nombreEjecutable).Length > 0;

    public static void SolicitarCierre(string nombreEjecutable)
    {
        Process[] procesos = ObtenerProcesos(nombreEjecutable);
        Debug.WriteLine($"CIERRE -> {nombreEjecutable}: {procesos.Length} proceso(s) " +
            $"[{string.Join(", ", procesos.Select(p => $"PID {p.Id} MainWindow={(p.MainWindowHandle != IntPtr.Zero)}"))}]");

        foreach (Process proceso in procesos)
        {
            if (proceso.MainWindowHandle != IntPtr.Zero)
            {
                bool solicitado = proceso.CloseMainWindow();
                Debug.WriteLine($"  CloseMainWindow PID {proceso.Id} -> {solicitado}");
            }
        }

        var timerGracia = new DispatcherTimer { Interval = PeriodoGracia };
        timerGracia.Tick += (_, _) =>
        {
            timerGracia.Stop();
            foreach (Process proceso in ObtenerProcesos(nombreEjecutable))
            {
                if (!proceso.HasExited)
                {
                    Debug.WriteLine($"  CIERRE FORZADO Kill() PID {proceso.Id}");
                    proceso.Kill();
                }
            }
        };
        timerGracia.Start();
    }

    private static Process[] ObtenerProcesos(string nombreEjecutable)
    {
        string nombreSinExtension = nombreEjecutable.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
            ? nombreEjecutable[..^4]
            : nombreEjecutable;

        return Process.GetProcessesByName(nombreSinExtension);
    }
}