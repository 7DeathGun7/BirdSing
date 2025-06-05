using BirdSing.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class CompraTutor
{
    [Key]
    public int IdCompraTutor { get; set; }

    [ForeignKey("Alumno")]
    public int IdAlumno { get; set; }
    public Alumno? Alumno { get; set; }

    [Required]
    public decimal Monto { get; set; }

    [Required]
    public string Tipo { get; set; } = "Compra";

    public DateTime Fecha { get; set; } = DateTime.Now;
}
