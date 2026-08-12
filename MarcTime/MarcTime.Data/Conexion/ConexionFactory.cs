using System.Data;
using Microsoft.Data.SqlClient;

namespace MarcTime.Data.Conexion;

/// <summary>
/// Abstrae de donde sale una conexion a SQL Server. Los repositorios piden
/// una conexion abierta sin saber de donde viene la cadena de conexion.
/// </summary>
public interface IConexionFactory
{
    IDbConnection CrearConexionAbierta();
}

public class ConexionFactory : IConexionFactory
{
    private readonly string _cadenaConexion;

    public ConexionFactory(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    public IDbConnection CrearConexionAbierta()
    {
        var conexion = new SqlConnection(_cadenaConexion);
        conexion.Open();
        return conexion;
    }
}