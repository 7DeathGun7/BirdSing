using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using BirdSing.Models;

namespace BirdSing.Models.ViewModels
{
    public class MisAlumnosViewModel
    {
        // Filtros
        public int? GradoId { get; set; }
        public int? GrupoId { get; set; }
        public int? MateriaId { get; set; }

        // Listas para los dropdowns
        public List<SelectListItem> Grados { get; set; } = new();
        public List<SelectListItem> Grupos { get; set; } = new();
        public List<SelectListItem> Materias { get; set; } = new();

        // Resultado
        public List<Alumno> Alumnos { get; set; } = new();
    }
}
