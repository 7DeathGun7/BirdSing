using System.ComponentModel.DataAnnotations;

namespace BirdSing.Models.ModelosViews
{
    public class UpdateMateriaDocenteViewModel
    {
        // antiguos
        public int OldIdDocente { get; set; }
        public int OldIdMateria { get; set; }

        [Display(Name = "Docente"), Required]
        public int IdDocente { get; set; }

        [Display(Name = "Materia"), Required]
        public int IdMateria { get; set; }
    }
}
