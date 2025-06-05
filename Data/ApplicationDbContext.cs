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
        public DbSet<AsignacionDocente> AsignacionDocentes { get; set; }
        public DbSet<CompraTutor> CompraTutores { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //
            // 1) Configuración de Avisos (todas con Restrict para no propagar el borrado)
            //
            modelBuilder.Entity<Aviso>()
                .HasOne(a => a.Docente)
                .WithMany(d => d.Avisos)
                .HasForeignKey(a => a.IdDocente)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Aviso>()
                .HasOne(a => a.Tutor)
                .WithMany(t => t.Avisos)
                .HasForeignKey(a => a.IdTutor)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Aviso>()
                .HasOne(a => a.Grupo)
                .WithMany(g => g.Avisos)
                .HasForeignKey(a => a.IdGrupo)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Aviso>()
                .HasOne(a => a.Materia)
                .WithMany(m => m.Avisos)
                .HasForeignKey(a => a.IdMateria)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Aviso>()
                .HasOne(a => a.Alumno)
                .WithMany(al => al.Avisos)
                .HasForeignKey(a => a.MatriculaAlumno)
                .OnDelete(DeleteBehavior.Restrict);


            //
            // 2) AlumnoTutor (tabla intermedia)
            //
            modelBuilder.Entity<AlumnoTutor>()
                .HasKey(at => new { at.MatriculaAlumno, at.IdTutor });

            modelBuilder.Entity<AlumnoTutor>()
                .HasOne(at => at.Alumno)
                .WithMany(a => a.AlumnosTutores)
                .HasForeignKey(at => at.MatriculaAlumno)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AlumnoTutor>()
                .HasOne(at => at.Tutor)
                .WithMany(t => t.AlumnosTutores)
                .HasForeignKey(at => at.IdTutor)
                .OnDelete(DeleteBehavior.Cascade);


            //
            // 3) Relación Alumno → Usuario (no cascada)
            //
            modelBuilder.Entity<Alumno>()
                .HasOne(a => a.Usuario)
                .WithMany(u => u.Alumnos)
                .HasForeignKey(a => a.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);


            //
            // 4) Relación Alumno → Grupo (no cascada)
            //
            modelBuilder.Entity<Alumno>()
                .HasOne(a => a.Grupo)
                .WithMany(g => g.Alumnos)
                .HasForeignKey(a => a.IdGrupo)
                .OnDelete(DeleteBehavior.Restrict);

            //
            // 5) Relación Alumno → Grado (no cascada)
            //
            modelBuilder.Entity<Alumno>()
                .HasOne(a => a.Grado)
                .WithMany()        // o .WithMany(g=>g.Alumnos) si Grado tiene navegación
                .HasForeignKey(a => a.IdGrado)
                .OnDelete(DeleteBehavior.Restrict);


            //
            // 6) MateriaDocente (tabla intermedia)
            //
            modelBuilder.Entity<MateriaDocente>()
                .HasKey(md => new { md.IdDocente, md.IdMateria });

            modelBuilder.Entity<MateriaDocente>()
                .HasOne(md => md.Docente)
                .WithMany(d => d.MateriasDocentes)
                .HasForeignKey(md => md.IdDocente)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MateriaDocente>()
                .HasOne(md => md.Materia)
                .WithMany(m => m.MateriasDocentes)
                .HasForeignKey(md => md.IdMateria)
                .OnDelete(DeleteBehavior.Cascade);


            //
            // 7) GrupoMateria (tabla intermedia)
            //
            modelBuilder.Entity<GrupoMateria>()
                .HasKey(gm => new { gm.IdGrupo, gm.IdMateria });

            modelBuilder.Entity<GrupoMateria>()
                .HasOne(gm => gm.Grupo)
                .WithMany(g => g.GrupoMaterias)
                .HasForeignKey(gm => gm.IdGrupo)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GrupoMateria>()
                .HasOne(gm => gm.Materia)
                .WithMany(m => m.GrupoMaterias)
                .HasForeignKey(gm => gm.IdMateria)
                .OnDelete(DeleteBehavior.Restrict);


            //
            // 8) DocenteGrupo (tabla intermedia)
            //
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

            //
            // 9) AsignacionDocente (relación completa con restricciones)
            //
            modelBuilder.Entity<AsignacionDocente>()
             .HasOne(a => a.Docente)
             .WithMany(d => d.Asignaciones)
             .HasForeignKey(a => a.IdDocente)
             .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AsignacionDocente>()
                .HasOne(a => a.Materia)
                .WithMany(m => m.Asignaciones)
                .HasForeignKey(a => a.IdMateria)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AsignacionDocente>()
                .HasOne(a => a.Grupo)
                .WithMany(g => g.Asignaciones)
                .HasForeignKey(a => a.IdGrupo)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AsignacionDocente>()
            .HasIndex(a => new { a.IdDocente, a.IdGrupo, a.IdMateria })
            .IsUnique();




            modelBuilder.Entity<Usuario>().HasQueryFilter(u => u.Activo);
            modelBuilder.Entity<Tutor>().HasQueryFilter(t => t.Activo);
            modelBuilder.Entity<Alumno>().HasQueryFilter(a => a.Activo);
            modelBuilder.Entity<Docente>().HasQueryFilter(d => d.Activo);
            modelBuilder.Entity<Materia>().HasQueryFilter(m => m.Activo);
            modelBuilder.Entity<Grupo>().HasQueryFilter(g => g.Activo);
            modelBuilder.Entity<Grado>().HasQueryFilter(g => g.Activo);
            modelBuilder.Entity<Aviso>().HasQueryFilter(a => a.Activo);
        }
    }
}
