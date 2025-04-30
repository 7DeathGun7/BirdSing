using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using BirdSing.Models;
using System.ComponentModel.DataAnnotations;

namespace BirdSing.Models.ViewModels
{
    public class MisAvisosViewModel
    {
        // Filtros
        [Display(Name = "Materia")]
        public int? MateriaId { get; set; }

        [Display(Name = "Alumno")]
        public int? AlumnoId { get; set; }

        [Display(Name = "Desde")]
        public DateTime? FechaDesde { get; set; }

        [Display(Name = "Hasta")]
        public DateTime? FechaHasta { get; set; }

        // Listas para los selects
        public List<SelectListItem> Materias { get; set; } = new();
        public List<SelectListItem> Alumnos { get; set; } = new();

        // Resultado
        public List<Aviso> Avisos { get; set; } = new();
    }
}
