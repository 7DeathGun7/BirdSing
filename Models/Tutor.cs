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

        [StringLength(20)]
        public string? Telefono { get; set; }

        [StringLength(255)]
        public string? Direccion { get; set; }

        public ICollection<AlumnoTutor> AlumnosTutores { get; set; } = new List<AlumnoTutor>();
        public ICollection<Aviso> Avisos { get; set; } = new List<Aviso>();
    }

}
