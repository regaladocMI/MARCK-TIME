using Dapper;
using MarcTime.Data.TypeHandlers;

namespace MarcTime.Data;

/// <summary>
/// Punto unico de registro de configuracion global de Dapper. Llamar UNA
/// SOLA VEZ al arrancar la app, antes de cualquier consulta (ver App.xaml.cs).
/// </summary>
public static class DapperConfiguracion
{
    private static bool _registrado;

    public static void Registrar()
    {
        if (_registrado) return;

        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler());

        _registrado = true;
    }
}