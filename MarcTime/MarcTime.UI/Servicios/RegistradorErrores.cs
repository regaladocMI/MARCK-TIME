using System.IO;

namespace MarcTime.UI.Servicios;

/// <summary>
/// Registra errores no controlados en un archivo de texto plano en
/// %AppData%\MarcTime\logs\ - suficiente para un proyecto de este tamano;
/// no se justifica traer una libreria de logging para esto.
/// </summary>
public static class RegistradorErrores
{
    private static readonly string CarpetaLogs = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MarcTime", "logs");

    public static void Registrar(Exception ex, string contexto)
    {
        try
        {
            Directory.CreateDirectory(CarpetaLogs);
            string archivo = Path.Combine(CarpetaLogs, $"error_{DateTime.Now:yyyyMMdd}.log");

            string entrada = $"""
                [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {contexto}
                {ex.GetType().FullName}: {ex.Message}
                {ex.StackTrace}
                ----------------------------------------

                """;

            File.AppendAllText(archivo, entrada);
        }
        catch
        {
            // Si ni siquiera se puede escribir el log (disco lleno, permisos,
            // etc.), no hay nada mas que hacer aqui - no relanzar, para no
            // generar un segundo error dentro del manejador de errores.
        }
    }
}