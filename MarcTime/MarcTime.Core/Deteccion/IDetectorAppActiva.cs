namespace MarcTime.Core.Deteccion;

public interface IDetectorAppActiva
{
    /// <summary>
    /// Devuelve la app en primer plano ahora mismo, o null si no se pudo
    /// determinar (ej. el proceso se cerro justo en ese instante).
    /// </summary>
    AppActivaInfo? ObtenerAppActiva();
}