// Models/ViewModels/CrearAvisoViewModel.cs
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BirdSing.Models.ViewModels
{
    public class CrearAvisoViewModel
    {
        [Required]
        public string ModoEnvio { get; set; }

        [Required]
        [Display(Name = "Grado – Grupo")]
        public int? IdGrupo { get; set; }

        [Display(Name = "Alumno")]
        public int? MatriculaAlumno { get; set; }

        [Required, StringLength(255)]
        public string Titulo { get; set; }

        public string Mensaje { get; set; }

        // listas para los selects
        public List<SelectListItem> Grupos { get; set; } = new();
        public List<SelectListItem> Materias { get; set; } = new();
        public List<SelectListItem> Alumnos { get; set; } = new();
    }
}
