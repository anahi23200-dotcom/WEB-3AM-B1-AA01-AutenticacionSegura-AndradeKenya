using System.Security.Claims;
using Taller.Datos.Entities;

namespace Taller.Negocio.Interfaces
{
    public record ResultadoOperacion(bool Exito, string Mensaje, ApplicationUser? Usuario = null);

    public interface IAuthService
    {
        Task<ResultadoOperacion> RegistrarClienteAsync(string nombreCompleto, string email, string password);
        Task<ResultadoOperacion> IniciarSesionAsync(string email, string password, bool recordarme);
        Task CerrarSesionAsync();
        Task<ResultadoOperacion> EnviarEnlaceRecuperacionAsync(string email, string callbackUrlBase);
        Task<ResultadoOperacion> RestablecerContrasenaAsync(string email, string token, string nuevaPassword);
        Task<ResultadoOperacion> ObtenerPerfilAsync(ClaimsPrincipal userPrincipal);
        Task<ResultadoOperacion> ActualizarPerfilAsync(ClaimsPrincipal userPrincipal, string nombreCompleto, string? phoneNumber);
    }
}