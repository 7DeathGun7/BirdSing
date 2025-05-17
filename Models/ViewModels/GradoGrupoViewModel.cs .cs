using System.ComponentModel.DataAnnotations;

namespace BirdSing.Models.ModelosViews
{
    public class GradoGrupoViewModel
    {
        // Para la edición
        public int IdGrupo { get; set; }

        [Required]
        [Display(Name = "Grado")]
        public int IdGrado { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Nombre del Grupo")]
        public string NombreGrupo { get; set; } = string.Empty;
    }
}

