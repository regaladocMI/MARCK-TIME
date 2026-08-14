using System.Data;
using Dapper;

namespace MarcTime.Data.TypeHandlers;

/// <summary>
/// Dapper no sabe convertir DateOnly/TimeOnly (tipos de .NET 6+) hacia/desde
/// los tipos DATE/TIME de SQL Server por defecto - hay que registrar estos
/// manejadores una sola vez al arrancar la app (ver DapperTypeHandlers.Registrar()).
/// </summary>
public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override DateOnly Parse(object value) => DateOnly.FromDateTime((DateTime)value);

    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.DbType = DbType.Date;
        parameter.Value = value.ToDateTime(TimeOnly.MinValue);
    }
}

public class TimeOnlyTypeHandler : SqlMapper.TypeHandler<TimeOnly>
{
    public override TimeOnly Parse(object value) => TimeOnly.FromTimeSpan((TimeSpan)value);

    public override void SetValue(IDbDataParameter parameter, TimeOnly value)
    {
        parameter.DbType = DbType.Time;
        parameter.Value = value.ToTimeSpan();
    }
}