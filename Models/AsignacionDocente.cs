using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BirdSing.Models
{
    public class AsignacionDocente
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo Docente es obligatorio.")]
        [Display(Name = "Docente")]
        public int IdDocente { get; set; }

        [Required(ErrorMessage = "El campo Grado es obligatorio.")]
        [Display(Name = "Grado")]
        public int IdGrado { get; set; }

        [Required(ErrorMessage = "El campo Grupo es obligatorio.")]
        [Display(Name = "Grupo")]
        public int IdGrupo { get; set; }

        [Required(ErrorMessage = "El campo Materia es obligatorio.")]
        [Display(Name = "Materia")]
        public int IdMateria { get; set; }

        public bool Activo { get; set; } = true;

        // Relaciones
        public Docente? Docente { get; set; }
        public Grupo? Grupo { get; set; }
        public Materia? Materia { get; set; }
    }
}
