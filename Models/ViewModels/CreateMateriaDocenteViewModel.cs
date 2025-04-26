using System.ComponentModel.DataAnnotations;

namespace BirdSing.Models.ModelosViews
{
    public class CreateMateriaDocenteViewModel
    {
        [Display(Name = "Docente"), Required]
        public int IdDocente { get; set; }

        [Display(Name = "Materia"), Required]
        public int IdMateria { get; set; }
    }
}
