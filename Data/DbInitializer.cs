using System.Linq;
using BirdSing.Models;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace BirdSing.Data
{
    public class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Rol { Nombre = "Administrador", Descripcion = "Rol de administrador del sistema" },
                    new Rol { Nombre = "Docente", Descripcion = "Rol de docente" },
                    new Rol { Nombre = "Tutor", Descripcion = "Rol de tutor" }
                );
                context.SaveChanges();
            }

            if (!context.Usuarios.Any(u => u.Email == "darwinMP@gmail.com"))
            {
                context.Usuarios.Add(new Usuario
                {
                    NombreUsuario = "Darwin Omar",
                    ApellidoPaterno = "Morales",
                    ApellidoMaterno = "Pech",
                    Email = "darwinMP@gmail.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("Admin123"),
                    IdRol = 1
                });
                context.SaveChanges();
            }
        }
    }
}