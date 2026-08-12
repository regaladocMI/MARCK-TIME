using System.Windows;
using Microsoft.Extensions.Configuration;
using MarcTime.Core.Models.Monitoreo;
using MarcTime.Data.Conexion;
using MarcTime.Data.Repositories;

namespace MarcTime.UI;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        EjecutarPruebaCrud();
        base.OnStartup(e);
    }

    private void EjecutarPruebaCrud()
    {
        var configuracion = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        string cadenaConexion = configuracion.GetConnectionString("MarcTimeDB")
            ?? throw new InvalidOperationException("Falta la cadena de conexion 'MarcTimeDB' en appsettings.");

        var fabricaConexion = new ConexionFactory(cadenaConexion);
        var repositorio = new AplicacionRepository(fabricaConexion);

        // TODO: reemplaza este valor por el UsuarioId real que obtuviste en el paso 4
        const int usuarioPruebaId = 1;

        int nuevoId = repositorio.Crear(new Aplicacion
        {
            UsuarioId = usuarioPruebaId,
            NombreEjecutable = "chrome.exe",
            NombreVisible = "Google Chrome",
            LimiteMinutosDiarios = 120,
            Activo = true
        });

        var todas = repositorio.ObtenerTodas(usuarioPruebaId);
        var creada = repositorio.ObtenerPorId(nuevoId);

        bool actualizada = false;
        if (creada is not null)
        {
            creada.LimiteMinutosDiarios = 90;
            actualizada = repositorio.Actualizar(creada);
        }

        bool eliminada = repositorio.Eliminar(nuevoId);

        MessageBox.Show(
            $"1. CREATE -> Id generado: {nuevoId}\n" +
            $"2. READ (todas) -> {todas.Count} aplicacion(es) del usuario\n" +
            $"3. READ (por id) -> {(creada is not null ? "encontrada: " + creada.NombreVisible : "NO encontrada")}\n" +
            $"4. UPDATE (limite a 90 min) -> {(actualizada ? "OK" : "FALLO")}\n" +
            $"5. DELETE -> {(eliminada ? "OK" : "FALLO")}",
            "MARC TIME - Prueba Seccion 4",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}