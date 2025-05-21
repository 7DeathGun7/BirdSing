using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BirdSing.Models;

namespace BirdSing.Models
{
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }

        [ForeignKey("Rol")]
        public int IdRol { get; set; }
        public Rol? Rol { get; set; }

        [Required]
        [StringLength(100)]
        public string NombreUsuario { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string ApellidoPaterno { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string ApellidoMaterno { get; set; } = null!;

        [Required]
        [EmailAddress(ErrorMessage = "Correo electrónico inválido.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        ErrorMessage = "El correo electrónico debe tener un formato válido.")]
        [StringLength(100)]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(255)]
        public string Password { get; set; } = null!;
        public bool Activo { get; set; } = true;


        public ICollection<Alumno> Alumnos { get; set; } = new List<Alumno>();


    }

}
