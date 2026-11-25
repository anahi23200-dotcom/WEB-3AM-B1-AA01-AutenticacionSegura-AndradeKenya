# Sistema de Autenticación Segura — Taller Automotriz (`TallerAutomotrizAuthDB`)

**Repositorio:** `WEB-3AM-B1-AA01-AutenticacionSegura-AndradeKenya`

## Arquitectura en 3 Capas
* **Capa de Datos (`Taller.Datos`):** Entidades (`ApplicationUser`, `ApplicationRole`), `ApplicationDbContext` con ASP.NET Core Identity y Entity Framework Core, migraciones para SQL Server y seeding inicial (`DbInitializer`) para roles (`Administrador`, `Cliente`) y cuenta administrativa base.
* **Capa de Negocio (`Taller.Negocio`):** Interfaces y servicios (`IAuthService` / `AuthService`, `IUsuarioService` / `UsuarioService`), validación de estado de cuenta (`Activo`), control de bloqueo, traducción de mensajes de error al español y envío de correos de recuperación mediante `IEmailService` / `SmtpEmailService`.
* **Capa de Presentación (`Taller.Presentacion`):** ViewModels (`LoginViewModel`, `RegistroViewModel`, `RecuperarContrasenaViewModel`, `RestablecerContrasenaViewModel`, `PerfilViewModel`), `AccountController`, `UsuariosController` (restringido al rol `Administrador`) y vistas Razor.

## Configuración de Secretos (`User Secrets`)
Las cadenas de conexión y credenciales SMTP no se almacenan en el código ni en `appsettings.json`. Configúralas en **Administrar secretos de usuario** (`secrets.json`) de `Taller.Presentacion`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=TallerAutomotrizAuthDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "SmtpSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "EnableSsl": true,
    "RemitenteCorreo": "tu-correo@gmail.com",
    "RemitenteNombre": "Taller Automotriz - Soporte",
    "Usuario": "tu-correo@gmail.com",
    "Contrasena": "TU_CONTRASEÑA_DE_APLICACION"
  }
}
