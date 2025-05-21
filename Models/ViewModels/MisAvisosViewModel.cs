using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using BirdSing.Models;

namespace BirdSing.Models.ViewModels
{
    public class MisAvisosViewModel
    {
        public string? GradoGrupoSeleccionado { get; set; }
        public int? AlumnoId { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string? NombreAlumno { get; set; }

        public List<SelectListItem> GradosGrupos { get; set; } = new();
        public List<SelectListItem> Alumnos { get; set; } = new();
        public List<Aviso> Avisos { get; set; } = new();
    }
}
