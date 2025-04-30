using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace BirdSing.Models.ViewModels
{
    public class MisAvisosTutorViewModel
    {
        // filtros
        public int? AlumnoId { get; set; }
        public bool? Leido { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }

        // listas para los dropdowns
        public List<SelectListItem> Alumnos { get; set; } = new();
        public List<SelectListItem> Estados { get; set; } = new();

        // los avisos filtrados
        public List<Aviso> Avisos { get; set; } = new();
    }
}
