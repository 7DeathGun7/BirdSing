using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BirdSing.Models;

namespace BirdSing.Models
{
    public class Aviso
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Docente")]
        public int IdDocente { get; set; }
        public Docente? Docente { get; set; }

        [ForeignKey("Tutor")]
        public int IdTutor { get; set; }
        public Tutor? Tutor { get; set; }

        [ForeignKey("Grupo")]
        public int IdGrupo { get; set; }
        public Grupo? Grupo { get; set; }

        [ForeignKey("Materia")]
        public int IdMateria { get; set; }
        public Materia? Materia { get; set; }

        [ForeignKey("Alumno")]
        public int? MatriculaAlumno { get; set; }
        public Alumno? Alumno { get; set; }

        [Required]
        [StringLength(255)]
        public string Titulo { get; set; } = null!;

        public string? Mensaje { get; set; }

        public DateTime Fecha { get; set; }

        public bool Leido { get; set; }

        [StringLength(50)]
        public string? TipoAviso { get; set; }
    }

}
