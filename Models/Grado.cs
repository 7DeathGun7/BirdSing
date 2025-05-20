using System.ComponentModel.DataAnnotations;

namespace BirdSing.Models
{
    public class Grado
    {
        [Key]
        public int IdGrado { get; set; }

        [StringLength(50)]
        public string? Grados { get; set; }

        public bool Activo { get; set; } = true;

        public ICollection<Grupo> Grupos { get; set; } = new List<Grupo>();

    }

}
