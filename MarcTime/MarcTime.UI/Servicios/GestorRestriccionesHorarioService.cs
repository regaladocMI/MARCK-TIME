using System.Diagnostics;
using MarcTime.Core.Consultas;
using MarcTime.Data.Repositories;
using MarcTime.UI.Vistas;

namespace MarcTime.UI.Servicios;

/// <summary>
/// Vigila las apps con restriccion de horario (Seccion 14): si una app
/// vigilada queda fuera de todos sus bloques permitidos, muestra una cuenta
/// regresiva y la cierra. A diferencia del limite diario (Seccion 8), aqui
/// no hay "aviso previo" escalonado - el bloque simplemente termino, asi
/// que se va directo a la cuenta regresiva final.
/// </summary>
public class GestorRestriccionesHorarioService
{
    private const int SegundosCuentaRegresiva = 20;

    private readonly IRestriccionHorarioAppRepository _restriccionRepository;
    private readonly int _usuarioId;
    private readonly HashSet<int> _enProcesoDeCierre = new();

    public GestorRestriccionesHorarioService(IRestriccionHorarioAppRepository restriccionRepository, int usuarioId)
    {
        _restriccionRepository = restriccionRepository;
        _usuarioId = usuarioId;
    }

    public void RevisarRestricciones()
    {
        List<AppFueraDeHorario> appsFueraDeHorario = _restriccionRepository.ObtenerAppsFueraDeHorario(_usuarioId);
        var idsFueraDeHorario = appsFueraDeHorario.Select(a => a.AplicacionId).ToHashSet();

        // Si una app ya no aparece fuera de horario (entro a un bloque
        // permitido, o ya se cerro), se libera para poder volver a
        // vigilarla la proxima vez que se salga de su horario.
        _enProcesoDeCierre.RemoveWhere(id => !idsFueraDeHorario.Contains(id));

        foreach (AppFueraDeHorario app in appsFueraDeHorario)
        {
            if (!_enProcesoDeCierre.Add(app.AplicacionId))
            {
                continue; // ya se disparo el cierre para esta app, no repetir
            }

            Debug.WriteLine($"FUERA DE HORARIO -> {app.NombreVisible}: no pertenece a ningun bloque activo ahora.");

            var ventana = new CuentaRegresivaWindow(
                $"{app.NombreVisible} (fuera de tu horario asignado)",
                SegundosCuentaRegresiva,
                alFinalizar: () => CierreAppService.SolicitarCierre(app.NombreEjecutable));

            ventana.Show();
        }
    }
}