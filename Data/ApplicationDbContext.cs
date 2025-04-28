using Microsoft.EntityFrameworkCore;
using BirdSing.Models;

namespace BirdSing.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Grado> Grados { get; set; }
        public DbSet<Grupo> Grupos { get; set; }
        public DbSet<GrupoMateria> GrupoMaterias { get; set; }
        public DbSet<Materia> Materias { get; set; }
        public DbSet<Alumno> Alumnos { get; set; }
        public DbSet<Tutor> Tutores { get; set; }
        public DbSet<Docente> Docentes { get; set; }
        public DbSet<AlumnoTutor> AlumnosTutores { get; set; }
        public DbSet<MateriaDocente> MateriasDocentes { get; set; }
        public DbSet<Aviso> Avisos { get; set; }
        public DbSet<DocenteGrupo> DocentesGrupos { get; set; }




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Restricciones en Aviso
            modelBuilder.Entity<Aviso>()
                .HasOne(a => a.Docente)
                .WithMany()
                .HasForeignKey(a => a.IdDocente)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Aviso>()
                .HasOne(a => a.Tutor)
                .WithMany()
                .HasForeignKey(a => a.IdTutor)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Aviso>()
                .HasOne(a => a.Grupo)
                .WithMany()
                .HasForeignKey(a => a.IdGrupo)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Aviso>()
                .HasOne(a => a.Materia)
                .WithMany()
                .HasForeignKey(a => a.IdMateria)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Aviso>()
                .HasOne(a => a.Alumno)
                .WithMany()
                .HasForeignKey(a => a.MatriculaAlumno)
                .OnDelete(DeleteBehavior.Restrict);

            // Restricción en Alumno -> Grupo (evita ON DELETE CASCADE)
            modelBuilder.Entity<Alumno>()
                .HasOne(a => a.Grupo)
                .WithMany(g => g.Alumnos)
                .HasForeignKey(a => a.IdGrupo)
                .OnDelete(DeleteBehavior.Restrict);

            // Relaciones para AlumnoTutor
            modelBuilder.Entity<AlumnoTutor>()
                .HasKey(at => new { at.MatriculaAlumno, at.IdTutor });

            modelBuilder.Entity<AlumnoTutor>()
                .HasOne(at => at.Alumno)
                .WithMany(a => a.AlumnosTutores)
                .HasForeignKey(at => at.MatriculaAlumno);

            modelBuilder.Entity<AlumnoTutor>()
                .HasOne(at => at.Tutor)
                .WithMany(t => t.AlumnosTutores)
                .HasForeignKey(at => at.IdTutor);

            // Relaciones para MateriaDocente
            modelBuilder.Entity<MateriaDocente>()
                .HasKey(md => new { md.IdDocente, md.IdMateria });

            modelBuilder.Entity<MateriaDocente>()
                .HasOne(md => md.Docente)
                .WithMany(d => d.MateriasDocentes)
                .HasForeignKey(md => md.IdDocente);

            modelBuilder.Entity<MateriaDocente>()
                .HasOne(md => md.Materia)
                .WithMany(m => m.MateriasDocentes)
                .HasForeignKey(md => md.IdMateria);

            // Relaciones GrupoMateria
            modelBuilder.Entity<GrupoMateria>()
                .HasKey(gm => new { gm.IdGrupo, gm.IdMateria });

            modelBuilder.Entity<GrupoMateria>()
                 .HasOne(gm => gm.Grupo)
                 .WithMany(g => g.GrupoMaterias)
                 .HasForeignKey(gm => gm.IdGrupo)
                 .OnDelete(DeleteBehavior.Cascade);

            // 2) Al borrar una MATERIA, no eliminar en cascada GrupoMaterias (evitamos multiple cascade paths)
            modelBuilder.Entity<GrupoMateria>()
                .HasOne(gm => gm.Materia)
                .WithMany(m => m.GrupoMaterias)
                .HasForeignKey(gm => gm.IdMateria)
                .OnDelete(DeleteBehavior.Restrict);

            // Mapping DocenteGrupo
            modelBuilder.Entity<DocenteGrupo>()
                        .HasKey(dg => new { dg.IdDocente, dg.IdGrado, dg.IdGrupo });
           
            modelBuilder.Entity<DocenteGrupo>()
                        .HasOne(dg => dg.Docente)
                        .WithMany(d => d.GrupoAsignados)
                        .HasForeignKey(dg => dg.IdDocente)
                        .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<DocenteGrupo>()
                        .HasOne(dg => dg.Grado)
                        .WithMany()
                        .HasForeignKey(dg => dg.IdGrado)
                       .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<DocenteGrupo>()
                        .HasOne(dg => dg.Grupo)
                        .WithMany()
                        .HasForeignKey(dg => dg.IdGrupo)
                        .OnDelete(DeleteBehavior.Restrict);


        }
    }
}
