using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BirdSing.Models;

namespace BirdSing.Models
{
    public class AlumnoTutor
    {
        [Key]
        [Column(Order = 0)]
        public int MatriculaAlumno { get; set; }

        [Key]
        [Column(Order = 1)]
        public int IdTutor { get; set; }

        [Required]
        [StringLength(50)]
        public string Parentesco { get; set; } = null!;

        // Especifica explícitamente la propiedad de clave externa
        [ForeignKey("MatriculaAlumno")]  // Asegúrate de que coincida con la clave primaria de Alumno
        public Alumno? Alumno { get; set; }

        [ForeignKey("IdTutor")]  // Asegúrate de que coincida con la clave primaria de Tutor
        public Tutor? Tutor { get; set; }

        public bool Activo { get; set; } = true;
    }

}
