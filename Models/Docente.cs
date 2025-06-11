using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BirdSing.Models;

namespace BirdSing.Models
{
    public class Docente
    {
        [Key]
        public int IdDocente { get; set; }

        [ForeignKey("Usuario")]
        public int IdUsuario { get; set; }
        public Usuario? Usuario { get; set; }

        [StringLength(20)]
        public string? MatriculaSEP { get; set; }

        public bool Activo { get; set; } = true;

        public ICollection<MateriaDocente> MateriasDocentes { get; set; } = new List<MateriaDocente>();
        public ICollection<Aviso> Avisos { get; set; } = new List<Aviso>();
        public ICollection<DocenteGrupo> GrupoAsignados { get; set; } = new List<DocenteGrupo>();
        public ICollection<AsignacionDocente> Asignaciones { get; set; } = new List<AsignacionDocente>();



    }

}
