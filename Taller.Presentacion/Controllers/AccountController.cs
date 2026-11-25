using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Taller.Datos.Entities;
using Taller.Datos.Seed;
using Taller.Negocio.Interfaces;
using Taller.Presentacion.Models.Auth;

namespace Taller.Presentacion.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(IAuthService authService, UserManager<ApplicationUser> userManager)
        {
            _authService = authService;
            _userManager = userManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var resultado = await _authService.IniciarSesionAsync(model.Email, model.Password, model.Recordarme);
            if (!resultado.Exito)
            {
                ModelState.AddModelError(string.Empty, resultado.Mensaje);
                return View(model);
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            if (resultado.Usuario != null && await _userManager.IsInRoleAsync(resultado.Usuario, DbInitializer.RolAdministrador))
            {
                return RedirectToAction("Index", "Usuarios");
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new RegistroViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegistroViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var resultado = await _authService.RegistrarClienteAsync(model.NombreCompleto, model.Email, model.Password);
            if (!resultado.Exito)
            {
                ModelState.AddModelError(string.Empty, resultado.Mensaje);
                return View(model);
            }

            TempData["MensajeExito"] = resultado.Mensaje;
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.CerrarSesionAsync();
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View(new RecuperarContrasenaViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(RecuperarContrasenaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var callbackUrlBase = Url.Action(nameof(ResetPassword), "Account", null, Request.Scheme)
                ?? $"{Request.Scheme}://{Request.Host}/Account/ResetPassword";

            try
            {
                var resultado = await _authService.EnviarEnlaceRecuperacionAsync(model.Email, callbackUrlBase);
                TempData["MensajeExito"] = resultado.Mensaje;
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "No se pudo enviar el correo de recuperación. Verifica la configuración SMTP en User Secrets.");
                return View(model);
            }

            return RedirectToAction(nameof(ForgotPassword));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string? email, string? token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction(nameof(Login));
            }

            return View(new RestablecerContrasenaViewModel
            {
                Email = email,
                Token = token
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(RestablecerContrasenaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var resultado = await _authService.RestablecerContrasenaAsync(model.Email, model.Token, model.NuevaPassword);
            if (!resultado.Exito)
            {
                ModelState.AddModelError(string.Empty, resultado.Mensaje);
                return View(model);
            }

            TempData["MensajeExito"] = resultado.Mensaje;
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var resultado = await _authService.ObtenerPerfilAsync(User);
            if (!resultado.Exito || resultado.Usuario == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var roles = await _userManager.GetRolesAsync(resultado.Usuario);
            return View(new PerfilViewModel
            {
                NombreCompleto = resultado.Usuario.NombreCompleto,
                Email = resultado.Usuario.Email ?? string.Empty,
                PhoneNumber = resultado.Usuario.PhoneNumber,
                Rol = roles.FirstOrDefault() ?? DbInitializer.RolCliente
            });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(PerfilViewModel model)
        {
            var actual = await _authService.ObtenerPerfilAsync(User);
            if (actual.Usuario != null)
            {
                var roles = await _userManager.GetRolesAsync(actual.Usuario);
                model.Email = actual.Usuario.Email ?? string.Empty;
                model.Rol = roles.FirstOrDefault() ?? DbInitializer.RolCliente;
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var resultado = await _authService.ActualizarPerfilAsync(User, model.NombreCompleto, model.PhoneNumber);
            if (!resultado.Exito)
            {
                ModelState.AddModelError(string.Empty, resultado.Mensaje);
                return View(model);
            }

            TempData["MensajeExito"] = resultado.Mensaje;
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}