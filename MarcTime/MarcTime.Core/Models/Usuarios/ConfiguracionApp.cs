namespace MarcTime.Core.Models.Usuarios;

/// <summary>
/// Refleja ConfiguracionApp. Relacion 1 a 1 con Usuario (UsuarioId es unico
/// en la base de datos).
/// </summary>
public class ConfiguracionApp
{
    public int ConfiguracionAppId { get; set; }
    public int UsuarioId { get; set; }
    public string Tema { get; set; } = "Claro";
    public int MinutosAntelacionDefecto { get; set; } = 10;
    public DateTime FechaActualizacion { get; set; }
}