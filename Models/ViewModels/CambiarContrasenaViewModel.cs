using System.ComponentModel.DataAnnotations;

public class CambiarContrasenaViewModel
{
    [Required]
    [Display(Name = "Contraseña actual")]
    public string ContrasenaActual { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [Display(Name = "Nueva contraseña")]
    public string NuevaContrasena { get; set; }

    [Required]
    [Compare("NuevaContrasena")]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmarContrasena { get; set; }
}
