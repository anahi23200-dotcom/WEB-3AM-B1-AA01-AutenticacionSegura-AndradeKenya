using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Taller.Datos.Entities;
using Taller.Datos.Seed;
using Taller.Negocio.Interfaces;

namespace Taller.Negocio.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public UsuarioService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<List<UsuarioConRolDto>> ListarUsuariosAsync()
        {
            var usuarios = await _userManager.Users.OrderBy(u => u.NombreCompleto).ToListAsync();
            var lista = new List<UsuarioConRolDto>();

            foreach (var u in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(u);
                var rolPrincipal = roles.FirstOrDefault() ?? DbInitializer.RolCliente;
                lista.Add(new UsuarioConRolDto(u.Id, u.NombreCompleto, u.Email ?? "", rolPrincipal, u.Activo));
            }

            return lista;
        }

        private async Task<bool> EsUltimoAdministradorActivoAsync(ApplicationUser usuario)
        {
            if (!usuario.Activo) return false;
            var esAdmin = await _userManager.IsInRoleAsync(usuario, DbInitializer.RolAdministrador);
            if (!esAdmin) return false;

            var todosAdmins = await _userManager.GetUsersInRoleAsync(DbInitializer.RolAdministrador);
            var adminsActivos = todosAdmins.Count(a => a.Activo);
            return adminsActivos <= 1;
        }

        public async Task<ResultadoOperacion> CambiarEstadoCuentaAsync(string usuarioId, bool activo)
        {
            var usuario = await _userManager.FindByIdAsync(usuarioId);
            if (usuario == null)
            {
                return new ResultadoOperacion(false, "Usuario no encontrado.");
            }

            if (!activo && await EsUltimoAdministradorActivoAsync(usuario))
            {
                return new ResultadoOperacion(false, "No se puede deshabilitar al último administrador activo del sistema.");
            }

            usuario.Activo = activo;
            var resultado = await _userManager.UpdateAsync(usuario);
            if (!resultado.Succeeded)
            {
                return new ResultadoOperacion(false, "Error al actualizar el estado de la cuenta.");
            }

            var estadoTexto = activo ? "habilitada" : "deshabilitada";
            return new ResultadoOperacion(true, $"Cuenta {estadoTexto} correctamente.");
        }

        public async Task<ResultadoOperacion> CambiarRolUsuarioAsync(string usuarioId, string nuevoRol)
        {
            if (nuevoRol != DbInitializer.RolAdministrador && nuevoRol != DbInitializer.RolCliente)
            {
                return new ResultadoOperacion(false, "Rol no válido.");
            }

            if (!await _roleManager.RoleExistsAsync(nuevoRol))
            {
                return new ResultadoOperacion(false, "El rol especificado no existe.");
            }

            var usuario = await _userManager.FindByIdAsync(usuarioId);
            if (usuario == null)
            {
                return new ResultadoOperacion(false, "Usuario no encontrado.");
            }

            var rolesActuales = await _userManager.GetRolesAsync(usuario);
            if (rolesActuales.Contains(DbInitializer.RolAdministrador) &&
                nuevoRol != DbInitializer.RolAdministrador &&
                await EsUltimoAdministradorActivoAsync(usuario))
            {
                return new ResultadoOperacion(false, "No se puede quitar el rol de Administrador al último administrador activo.");
            }

            if (rolesActuales.Any())
            {
                await _userManager.RemoveFromRolesAsync(usuario, rolesActuales);
            }

            await _userManager.AddToRoleAsync(usuario, nuevoRol);
            return new ResultadoOperacion(true, $"Rol actualizado a {nuevoRol}.");
        }
    }
}