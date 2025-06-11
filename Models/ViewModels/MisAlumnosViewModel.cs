using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using BirdSing.Models;

namespace BirdSing.Models.ViewModels
{
    public class MisAlumnosViewModel
    {
        public int? GradoId { get; set; }
        public int? GrupoId { get; set; }
        public int? MateriaId { get; set; }
        public int? AlumnoId { get; set; }

        public List<SelectListItem> Grados { get; set; } = new();
        public List<SelectListItem> Grupos { get; set; } = new();
        public List<SelectListItem> Materias { get; set; } = new();
        public List<SelectListItem> AlumnosList { get; set; } = new();

        // Esta lista es la que usas para poblar la tabla
        public List<Alumno> Alumnos { get; set; } = new();
    }

}

