using System.Windows.Input;
using MarcTime.Core.Models.Enums;
using MarcTime.Data.Repositories;
using MarcTime.UI.Comun;
using MarcTime.UI.Vistas;

namespace MarcTime.UI.ViewModels;

using MarcTime.Core.Consultas;

/// <summary>
/// Pantalla de horario semanal: 7 columnas (Dias), cada una con sus bloques
/// (Cursos + HorariosClase de la Seccion 9). "Agregar bloque" abre
/// EditarBloqueWindow, que tambien permite ligar restricciones de app
/// (Seccion 14) al bloque nuevo.
/// </summary>
public class HorarioViewModel : ViewModelBase
{
    private readonly IHorarioClaseRepository _horarioRepository;
    private readonly ICursoRepository _cursoRepository;
    private readonly IAplicacionRepository _aplicacionRepository;
    private readonly IRestriccionHorarioAppRepository _restriccionRepository;
    private readonly int _usuarioId;

    public ICommand EditarBloqueCommand { get; }

    public HorarioViewModel(
        IHorarioClaseRepository horarioRepository,
        ICursoRepository cursoRepository,
        IAplicacionRepository aplicacionRepository,
        IRestriccionHorarioAppRepository restriccionRepository,
        int usuarioId)
    {
        _horarioRepository = horarioRepository;
        _cursoRepository = cursoRepository;
        _aplicacionRepository = aplicacionRepository;
        _restriccionRepository = restriccionRepository;
        _usuarioId = usuarioId;

        foreach (DiaSemana dia in Enum.GetValues<DiaSemana>())
        {
            Dias.Add(new DiaHorarioViewModel(dia));
        }

        AgregarBloqueCommand = new RelayCommand<DiaSemana>(AgregarBloque);
        EliminarBloqueCommand = new RelayCommand<int>(EliminarBloque);

        EditarBloqueCommand = new RelayCommand<int>(EditarBloque);

        Cargar();
    }

    public List<DiaHorarioViewModel> Dias { get; } = new();
    public ICommand AgregarBloqueCommand { get; }
    public ICommand EliminarBloqueCommand { get; }

    private void Cargar()
    {
        foreach (var diaVm in Dias)
        {
            diaVm.Bloques.Clear();
        }

        foreach (var bloque in _horarioRepository.ObtenerDetalladoPorUsuario(_usuarioId))
        {
            var columna = Dias.FirstOrDefault(d => d.Dia == bloque.DiaSemana);
            columna?.Bloques.Add(bloque);
        }
    }

    private void AgregarBloque(DiaSemana dia)
    {
        var ventana = new EditarBloqueWindow(
            dia, _cursoRepository, _aplicacionRepository, _restriccionRepository, _horarioRepository, _usuarioId);

        if (ventana.ShowDialog() == true)
        {
            Cargar(); // refresca todas las columnas tras guardar
        }
    }


    private void EditarBloque(int horarioClaseId)
    {
        var bloque = Dias.SelectMany(d => d.Bloques).FirstOrDefault(b => b.HorarioClaseId == horarioClaseId);
        if (bloque is null) return;

        var ventana = new EditarBloqueWindow(
            bloque.DiaSemana, _cursoRepository, _aplicacionRepository, _restriccionRepository, _horarioRepository, _usuarioId,
            bloqueExistente: bloque);

        if (ventana.ShowDialog() == true)
        {
            Cargar();
        }
    }

    private void EliminarBloque(int horarioClaseId)
    {
        _horarioRepository.Eliminar(horarioClaseId); // RestriccionesHorarioApp se borra sola (CASCADE, Seccion 14)
        Cargar();
    }
}