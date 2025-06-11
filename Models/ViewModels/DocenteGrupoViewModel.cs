namespace BirdSing.Models.ViewModels
{
    public class DocenteGrupoViewModel
    {
        public int IdDocente { get; set; }
        public string NombreDocente { get; set; } = null!;
        public int IdGrado { get; set; }
        public string NombreGrado { get; set; } = null!;
        public int IdGrupo { get; set; }
        public string NombreGrupo { get; set; } = null!;
    }
}
