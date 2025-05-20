using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BirdSing.Models
{
    public class MateriaDocente
    {
        // Si tienes Id como PK:
        public int Id { get; set; }

        public int IdDocente { get; set; }
        public Docente? Docente { get; set; }

        public int IdMateria { get; set; }
        public Materia? Materia { get; set; }

        // Claves antiguas para edición
        [NotMapped]
        public int OldIdDocente { get; set; }
        [NotMapped]
        public int OldIdMateria { get; set; }

        public bool Activo { get; set; } = true;
    }
}
