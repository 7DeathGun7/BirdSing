using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BirdSing.Models
{
    public class GrupoMateria
    {
        [Key, Column(Order = 0)]
        public int IdGrupo { get; set; }
        [ForeignKey(nameof(IdGrupo))]
        public Grupo? Grupo { get; set; }

        [Key, Column(Order = 1)]
        public int IdMateria { get; set; }
        [ForeignKey(nameof(IdMateria))]
        public Materia? Materia { get; set; }

        public bool Activo { get; set; } = true;

    }
}
