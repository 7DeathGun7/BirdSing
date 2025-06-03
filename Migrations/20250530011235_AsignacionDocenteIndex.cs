using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BirdSing.Migrations
{
    /// <inheritdoc />
    public partial class AsignacionDocenteIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AsignacionDocentes_IdDocente",
                table: "AsignacionDocentes");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionDocentes_IdDocente_IdGrupo_IdMateria",
                table: "AsignacionDocentes",
                columns: new[] { "IdDocente", "IdGrupo", "IdMateria" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AsignacionDocentes_IdDocente_IdGrupo_IdMateria",
                table: "AsignacionDocentes");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionDocentes_IdDocente",
                table: "AsignacionDocentes",
                column: "IdDocente");
        }
    }
}
