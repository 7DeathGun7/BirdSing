using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BirdSing.Models
{
    public class Alumno
    {
        [Key]
        public int MatriculaAlumno { get; set; }

        // -> NUEVO: FK a Usuario
        [ForeignKey("Usuario")]
        public int? IdUsuario { get; set; }
        public Usuario? Usuario { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string NombreAlumno { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string ApellidoPaterno { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string ApellidoMaterno { get; set; } = null!;

        public DateTime? FechaNacimiento { get; set; }

        [StringLength(20)]
        public string? Curp { get; set; }

        [ForeignKey("Grado")]
        public int IdGrado { get; set; }
        public Grado? Grado { get; set; }

        [ForeignKey("Grupo")]
        public int IdGrupo { get; set; }
        public Grupo? Grupo { get; set; }

        public ICollection<AlumnoTutor> AlumnosTutores { get; set; } = new List<AlumnoTutor>();
        public ICollection<Aviso> Avisos { get; set; } = new List<Aviso>();
    }
}
