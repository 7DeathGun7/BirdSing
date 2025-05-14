using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BirdSing.Models;

namespace BirdSing.Models
{
    public class Tutor
    {
        [Key]
        public int IdTutor { get; set; }

        [ForeignKey("Usuario")]
        public int IdUsuario { get; set; }
        public Usuario? Usuario { get; set; }


        [Required]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "El teléfono debe tener exactamente 10 dígitos numéricos.")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; } = null!;

        [StringLength(255)]
        public string? Direccion { get; set; }

        public ICollection<AlumnoTutor> AlumnosTutores { get; set; } = new List<AlumnoTutor>();
        public ICollection<Aviso> Avisos { get; set; } = new List<Aviso>();
    }

}
