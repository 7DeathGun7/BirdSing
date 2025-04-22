using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BirdSing.Models;

namespace BirdSing.Models
{
    public class MateriaDocente
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Docente")]
        public int IdDocente { get; set; }
        public Docente? Docente { get; set; }

        [ForeignKey("Materia")]
        public int IdMateria { get; set; }
        public Materia? Materia { get; set; }

    }

}
