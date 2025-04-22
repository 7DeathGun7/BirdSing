using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BirdSing.Models;

namespace BirdSing.Models
{
    public class Grupo
    {
        [Key]
        public int IdGrupo { get; set; }

        [ForeignKey("Grado")]
        public int IdGrado { get; set; }
        public Grado? Grado { get; set; }

        [StringLength(50)]
        public string? Grupos { get; set; }

        public ICollection<Alumno> Alumnos { get; set; }
    }

}
