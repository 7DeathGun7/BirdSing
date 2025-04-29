using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        // <- Agrega estas colecciones para facilitar Includes
        public ICollection<Alumno> Alumnos { get; set; } = new List<Alumno>();
        public ICollection<GrupoMateria> GrupoMaterias { get; set; } = new List<GrupoMateria>();
        public ICollection<DocenteGrupo> DocentesGrupos { get; set; } = new List<DocenteGrupo>();
        public ICollection<Aviso> Avisos { get; set; } = new List<Aviso>();
    }
}
