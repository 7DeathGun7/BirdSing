using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BirdSing.Models;

namespace BirdSing.Models
{
    public class Materia
    {
        [Key]
        public int IdMateria { get; set; }

        [ForeignKey("Grado")]
        public int IdGrado { get; set; }
        public Grado? Grado { get; set; }
        public ICollection<GrupoMateria> GrupoMaterias { get; set; } = new List<GrupoMateria>();


        [Required]
        [StringLength(100)]
        public string NombreMateria { get; set; } = null!;

        public bool Activo { get; set; } = true;
        public ICollection<MateriaDocente> MateriasDocentes { get; set; } = new List<MateriaDocente>();
        public ICollection<Aviso> Avisos { get; set; } = new List<Aviso>();
    }

}
