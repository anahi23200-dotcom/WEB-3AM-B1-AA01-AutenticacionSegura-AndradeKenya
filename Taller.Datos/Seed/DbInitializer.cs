using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Taller.Datos.Context;
using Taller.Datos.Entities;

namespace Taller.Datos.Seed
{
    public static class DbInitializer
    {
        public const string RolAdministrador = "Administrador";
        public const string RolCliente = "Cliente";

        public static async Task InicializarAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            // Asegurar que la base de datos exista y las migraciones estén aplicadas
            await context.Database.MigrateAsync();

            // 1. Crear roles obligatorios si no existen
            string[] roles = { RolAdministrador, RolCliente };
            foreach (var rol in roles)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                {
                    await roleManager.CreateAsync(new ApplicationRole(rol));
                }
            }

            // 2. Crear usuario Administrador inicial controlado (cuenta de prueba)
            const string adminEmail = "admin.taller@correo.com";
            const string adminPassword = "Admin123*";

            var adminUsuario = await userManager.FindByEmailAsync(adminEmail);
            if (adminUsuario == null)
            {
                adminUsuario = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    NombreCompleto = "Administrador Inicial del Taller",
                    EmailConfirmed = true,
                    Activo = true
                };

                var resultadoCreacion = await userManager.CreateAsync(adminUsuario, adminPassword);
                if (resultadoCreacion.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUsuario, RolAdministrador);
                }
            }
            else if (!await userManager.IsInRoleAsync(adminUsuario, RolAdministrador))
            {
                await userManager.AddToRoleAsync(adminUsuario, RolAdministrador);
            }
        }
    }
}