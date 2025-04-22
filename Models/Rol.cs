using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BirdSing.Models
{
    public class Rol
    {
        [Key]
        public int IdRol { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = null!;

        public string? Descripcion { get; set; }
    }

}
