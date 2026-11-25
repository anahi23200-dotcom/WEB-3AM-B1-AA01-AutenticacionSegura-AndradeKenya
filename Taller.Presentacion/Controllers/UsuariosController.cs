using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taller.Datos.Seed;
using Taller.Negocio.Interfaces;

namespace Taller.Presentacion.Controllers
{
    [Authorize(Roles = DbInitializer.RolAdministrador)]
    public class UsuariosController : Controller
    {
        private readonly IUsuarioService _usuarioService;

        public UsuariosController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var usuarios = await _usuarioService.ListarUsuariosAsync();
            return View(usuarios);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(string usuarioId, bool activo)
        {
            var resultado = await _usuarioService.CambiarEstadoCuentaAsync(usuarioId, activo);
            if (resultado.Exito)
            {
                TempData["MensajeExito"] = resultado.Mensaje;
            }
            else
            {
                TempData["MensajeError"] = resultado.Mensaje;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarRol(string usuarioId, string nuevoRol)
        {
            var resultado = await _usuarioService.CambiarRolUsuarioAsync(usuarioId, nuevoRol);
            if (resultado.Exito)
            {
                TempData["MensajeExito"] = resultado.Mensaje;
            }
            else
            {
                TempData["MensajeError"] = resultado.Mensaje;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}