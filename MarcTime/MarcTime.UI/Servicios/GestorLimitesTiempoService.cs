using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using MarcTime.Core.Consultas;
using MarcTime.Data.Repositories;
using MarcTime.UI.Vistas;

namespace MarcTime.UI.Servicios;

/// <summary>
/// Vigila el estado de limites de tiempo y actua en 2 momentos configurables:
///   1. Quedan "segundosAvisoPrevio" o menos -> AvisoLimiteWindow (no modal,
///      se puede ignorar, se autocierra sola).
///   2. Quedan "segundosCuentaRegresiva" o menos -> CuentaRegresivaWindow
///      con su propio timer de 1s; al llegar a 0 dispara el cierre real
///      (amable primero, forzado tras el periodo de gracia).
/// </summary>
public class GestorLimitesTiempoService
{
    private static readonly TimeSpan PeriodoGracia = TimeSpan.FromSeconds(15);

    private readonly IAplicacionRepository _aplicacionRepository;
    private readonly int _usuarioId;
    private readonly int _segundosAvisoPrevio;
    private readonly int _segundosCuentaRegresiva;

    private readonly HashSet<int> _aplicacionesAvisadas = new();
    private readonly HashSet<int> _cuentasRegresivasIniciadas = new();

    public GestorLimitesTiempoService(
        IAplicacionRepository aplicacionRepository,
        int usuarioId,
        int segundosAvisoPrevio = 60,
        int segundosCuentaRegresiva = 15)
    {
        _aplicacionRepository = aplicacionRepository;
        _usuarioId = usuarioId;
        _segundosAvisoPrevio = segundosAvisoPrevio;
        _segundosCuentaRegresiva = segundosCuentaRegresiva;
    }

    public void RevisarLimites()
    {
        List<EstadoLimiteAplicacion> estados = _aplicacionRepository.ObtenerEstadoLimites(_usuarioId);

        foreach (EstadoLimiteAplicacion estado in estados)
        {
            int? restantes = estado.SegundosRestantes;
            if (restantes is null)
            {
                continue;
            }

            if (!CierreAppService.EstaCorriendo(estado.NombreEjecutable))
            {
                continue; // no esta abierta, no hay nada que avisar ni cerrar
            }

            if (restantes <= 0)
            {
                // Salvavidas: si la app ya arranco por encima del limite y
                // por algun motivo no paso por la cuenta regresiva, cerrar directo.
                if (_cuentasRegresivasIniciadas.Add(estado.AplicacionId))
                {
                    Debug.WriteLine($"LIMITE YA SUPERADO -> {estado.NombreVisible}: cierre directo.");
                    EjecutarCierre(estado);
                }
            }
            else if (restantes <= _segundosCuentaRegresiva && _cuentasRegresivasIniciadas.Add(estado.AplicacionId))
            {
                MostrarCuentaRegresiva(estado, restantes.Value);
            }
            else if (restantes <= _segundosAvisoPrevio && _aplicacionesAvisadas.Add(estado.AplicacionId))
            {
                MostrarAviso(estado, restantes.Value);
            }
        }
    }

    private static void MostrarAviso(EstadoLimiteAplicacion estado, int segundosRestantes)
    {
        Debug.WriteLine($"AVISO -> {estado.NombreVisible}: quedan {segundosRestantes} segundo(s).");

        string mensaje = segundosRestantes >= 60
            ? $"Quedan {segundosRestantes / 60} minuto(s) de uso para {estado.NombreVisible} hoy."
            : $"Quedan {segundosRestantes} segundo(s) de uso para {estado.NombreVisible} hoy.";

        var ventana = new AvisoLimiteWindow(mensaje);
        ventana.Show(); // NO modal: evita el bug de reentrada del MessageBox
    }

    private void MostrarCuentaRegresiva(EstadoLimiteAplicacion estado, int segundosRestantes)
    {
        Debug.WriteLine($"CUENTA REGRESIVA -> {estado.NombreVisible}: {segundosRestantes}s hasta el cierre.");

        var ventana = new CuentaRegresivaWindow(
            estado.NombreVisible,
            segundosRestantes,
            alFinalizar: () => EjecutarCierre(estado));

        ventana.Show();
    }

    private void EjecutarCierre(EstadoLimiteAplicacion estado)
    {
        CierreAppService.SolicitarCierre(estado.NombreEjecutable);
    }

    private static Process[] ObtenerProcesosDeApp(string nombreEjecutable)
    {
        string nombreSinExtension = nombreEjecutable.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
            ? nombreEjecutable[..^4]
            : nombreEjecutable;

        return Process.GetProcessesByName(nombreSinExtension);
    }
}