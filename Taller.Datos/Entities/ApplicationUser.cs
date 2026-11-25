using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Taller.Datos.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(150)]
        public string NombreCompleto { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;
    }
}