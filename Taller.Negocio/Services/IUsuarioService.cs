using Taller.Datos.Entities;

namespace Taller.Negocio.Interfaces
{
    public record UsuarioConRolDto(
        string Id,
        string NombreCompleto,
        string Email,
        string Rol,
        bool Activo);

    public interface IUsuarioService
    {
        Task<List<UsuarioConRolDto>> ListarUsuariosAsync();
        Task<ResultadoOperacion> CambiarEstadoCuentaAsync(string usuarioId, bool activo);
        Task<ResultadoOperacion> CambiarRolUsuarioAsync(string usuarioId, string nuevoRol);
    }
}