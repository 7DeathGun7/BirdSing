using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BirdSing.Models
{
    public class DocenteGrupo
    {
        [Key, Column(Order = 0)]
        public int IdDocente { get; set; }
        [ForeignKey(nameof(IdDocente))]
        public Docente? Docente { get; set; }

        [Key, Column(Order = 1)]
        public int IdGrado { get; set; }
        [ForeignKey(nameof(IdGrado))]
        public Grado? Grado { get; set; }

        [Key, Column(Order = 2)]
        public int IdGrupo { get; set; }
        [ForeignKey(nameof(IdGrupo))]
        public Grupo? Grupo { get; set; }
    }
}
