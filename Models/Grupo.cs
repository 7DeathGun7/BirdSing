using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BirdSing.Models
{
    public class Grupo
    {
        [Key]
        public int IdGrupo { get; set; }

        [Required]
        [Display(Name = "Grado")]
        [ForeignKey("Grado")]
        public int IdGrado { get; set; }

        public Grado? Grado { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Nombre del Grupo")]
        public string Grupos { get; set; } = string.Empty;

        // Para que no intente validar o enlazar la colección de Alumnos
        [ValidateNever]
        public ICollection<Alumno> Alumnos { get; set; } = new List<Alumno>();
    }
}
