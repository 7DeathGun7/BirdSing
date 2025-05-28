using System.ComponentModel.DataAnnotations;

namespace BirdSing.Models
{
    public class AsignacionDocente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int IdDocente { get; set; }

        [Required]
        public int IdMateria { get; set; }

        [Required]
        public int IdGrupo { get; set; }

        public bool Activo { get; set; } = true;

        // Relaciones (si las tienes)
        public Docente Docente { get; set; }
        public Materia Materia { get; set; }
        public Grupo Grupo { get; set; }
    }
}
