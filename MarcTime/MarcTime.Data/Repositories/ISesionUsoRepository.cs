using MarcTime.Core.Models.Monitoreo;

namespace MarcTime.Data.Repositories;

public interface ISesionUsoRepository
{
    long AbrirSesion(int aplicacionId, DateTime fechaHoraInicio, string? tituloVentana);
    bool CerrarSesion(long sesionUsoId, DateTime fechaHoraFin);
    SesionUso? ObtenerSesionAbierta(int aplicacionId);
}