using System.Collections.ObjectModel;
using MarcTime.Core.Consultas;
using MarcTime.Core.Models.Enums;

namespace MarcTime.UI.ViewModels;

/// <summary>Una columna del horario semanal: un dia con su lista de bloques.</summary>
public class DiaHorarioViewModel
{
    public DiaHorarioViewModel(DiaSemana dia)
    {
        Dia = dia;
    }

    public DiaSemana Dia { get; }
    public string NombreDia => Dia.ToString();
    public ObservableCollection<BloqueHorarioDetalle> Bloques { get; } = new();
}