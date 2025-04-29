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
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(255)]
        public string Password { get; set; } = null!;

        public ICollection<Alumno> Alumnos { get; set; } = new List<Alumno>();


    }

}
