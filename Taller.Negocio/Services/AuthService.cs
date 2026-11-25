using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Taller.Datos.Entities;
using Taller.Datos.Seed;
using Taller.Negocio.Interfaces;

namespace Taller.Negocio.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
        }

        public async Task<ResultadoOperacion> RegistrarClienteAsync(string nombreCompleto, string email, string password)
        {
            var correo = email.Trim();
            var existente = await _userManager.FindByEmailAsync(correo);
            if (existente != null)
            {
                return new ResultadoOperacion(false, "Ya existe una cuenta registrada con ese correo electrónico.");
            }

            var usuario = new ApplicationUser
            {
                UserName = correo,
                Email = correo,
                NombreCompleto = nombreCompleto.Trim(),
                Activo = true,
                EmailConfirmed = true
            };

            var creacion = await _userManager.CreateAsync(usuario, password);
            if (!creacion.Succeeded)
            {
                var errores = string.Join(" ", creacion.Errors.Select(e => TraducirErrorIdentity(e.Description)));
                return new ResultadoOperacion(false, $"No se pudo crear la cuenta: {errores}");
            }

            // El registro público siempre asigna el rol Cliente
            await _userManager.AddToRoleAsync(usuario, DbInitializer.RolCliente);

            return new ResultadoOperacion(true, "Cuenta registrada correctamente. Ya puedes iniciar sesión.", usuario);
        }

        public async Task<ResultadoOperacion> IniciarSesionAsync(string email, string password, bool recordarme)
        {
            var correo = email.Trim();
            var usuario = await _userManager.FindByEmailAsync(correo);

            if (usuario == null)
            {
                return new ResultadoOperacion(false, "Credenciales inválidas.");
            }

            if (!usuario.Activo)
            {
                return new ResultadoOperacion(false, "Tu cuenta está deshabilitada. Contacta al administrador del taller.");
            }

            var resultado = await _signInManager.PasswordSignInAsync(
                usuario.UserName!,
                password,
                isPersistent: recordarme,
                lockoutOnFailure: true);

            if (resultado.Succeeded)
            {
                return new ResultadoOperacion(true, "Inicio de sesión exitoso.", usuario);
            }

            if (resultado.IsLockedOut)
            {
                return new ResultadoOperacion(false, "Cuenta bloqueada temporalmente por múltiples intentos fallidos.");
            }

            return new ResultadoOperacion(false, "Credenciales inválidas.");
        }

        public async Task CerrarSesionAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<ResultadoOperacion> EnviarEnlaceRecuperacionAsync(string email, string callbackUrlBase)
        {
            var correo = email.Trim();
            var usuario = await _userManager.FindByEmailAsync(correo);

            const string mensajeGenerico = "Si el correo existe y está activo en el sistema, hemos enviado un enlace de recuperación.";

            if (usuario == null || !usuario.Activo)
            {
                return new ResultadoOperacion(true, mensajeGenerico);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
            var separador = callbackUrlBase.Contains('?') ? "&" : "?";
            var enlace = $"{callbackUrlBase}{separador}email={Uri.EscapeDataString(usuario.Email!)}&token={Uri.EscapeDataString(token)}";

            var html = $@"
                <h3>Recuperación de Contraseña - Taller Automotriz</h3>
                <p>Hola <strong>{HtmlEncoder.Default.Encode(usuario.NombreCompleto)}</strong>,</p>
                <p>Recibimos una solicitud para restablecer tu contraseña. Haz clic en el siguiente enlace:</p>
                <p><a href=""{enlace}"">Restablecer contraseña</a></p>
                <p>Si el botón o enlace no abre directamente, copia y pega esta URL en tu navegador:<br/>{HtmlEncoder.Default.Encode(enlace)}</p>
                <p>Si no solicitaste este cambio, ignora este mensaje.</p>";

            await _emailService.EnviarCorreoAsync(usuario.Email!, "Recuperación de contraseña - Taller Automotriz", html);
            return new ResultadoOperacion(true, mensajeGenerico);
        }

        public async Task<ResultadoOperacion> RestablecerContrasenaAsync(string email, string token, string nuevaPassword)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            {
                return new ResultadoOperacion(false, "El enlace de restablecimiento es incompleto o inválido.");
            }

            var usuario = await _userManager.FindByEmailAsync(email.Trim());
            if (usuario == null)
            {
                return new ResultadoOperacion(false, "No fue posible restablecer la contraseña.");
            }

            var resultado = await _userManager.ResetPasswordAsync(usuario, token, nuevaPassword);
            if (!resultado.Succeeded)
            {
                var errores = string.Join(" ", resultado.Errors.Select(e => TraducirErrorIdentity(e.Description)));
                return new ResultadoOperacion(false, $"No se pudo restablecer la contraseña: {errores}");
            }

            return new ResultadoOperacion(true, "Contraseña actualizada correctamente. Ya puedes iniciar sesión.");
        }

        public async Task<ResultadoOperacion> ObtenerPerfilAsync(ClaimsPrincipal userPrincipal)
        {
            var usuario = await _userManager.GetUserAsync(userPrincipal);
            if (usuario == null)
            {
                return new ResultadoOperacion(false, "No se encontró la sesión del usuario.");
            }

            return new ResultadoOperacion(true, "Perfil cargado.", usuario);
        }

        public async Task<ResultadoOperacion> ActualizarPerfilAsync(ClaimsPrincipal userPrincipal, string nombreCompleto, string? phoneNumber)
        {
            var usuario = await _userManager.GetUserAsync(userPrincipal);
            if (usuario == null)
            {
                return new ResultadoOperacion(false, "No se encontró la sesión del usuario.");
            }

            usuario.NombreCompleto = nombreCompleto.Trim();
            usuario.PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();

            var resultado = await _userManager.UpdateAsync(usuario);
            if (!resultado.Succeeded)
            {
                return new ResultadoOperacion(false, "No se pudo actualizar el perfil.");
            }

            return new ResultadoOperacion(true, "Perfil actualizado correctamente.", usuario);
        }

        private static string TraducirErrorIdentity(string descripcion)
        {
            if (descripcion.Contains("uppercase", StringComparison.OrdinalIgnoreCase))
                return "La contraseña debe tener al menos una letra mayúscula ('A'-'Z').";
            if (descripcion.Contains("lowercase", StringComparison.OrdinalIgnoreCase))
                return "La contraseña debe tener al menos una letra minúscula ('a'-'z').";
            if (descripcion.Contains("digit", StringComparison.OrdinalIgnoreCase))
                return "La contraseña debe tener al menos un número ('0'-'9').";
            if (descripcion.Contains("non alphanumeric", StringComparison.OrdinalIgnoreCase))
                return "La contraseña debe incluir al menos un carácter especial.";
            if (descripcion.Contains("too short", StringComparison.OrdinalIgnoreCase) ||
                descripcion.Contains("at least", StringComparison.OrdinalIgnoreCase))
                return "La contraseña no cumple con la longitud mínima requerida.";
            if (descripcion.Contains("Invalid token", StringComparison.OrdinalIgnoreCase))
                return "El enlace o token de restablecimiento no es válido o ya expiró.";

            return descripcion;
        }
    }
}