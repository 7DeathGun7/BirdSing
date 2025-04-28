using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BirdSing.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureCascadeDeletes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GrupoMaterias_Grupos_IdGrupo",
                table: "GrupoMaterias");

            migrationBuilder.DropForeignKey(
                name: "FK_GrupoMaterias_Materias_IdMateria",
                table: "GrupoMaterias");

            migrationBuilder.AddForeignKey(
                name: "FK_GrupoMaterias_Grupos_IdGrupo",
                table: "GrupoMaterias",
                column: "IdGrupo",
                principalTable: "Grupos",
                principalColumn: "IdGrupo",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GrupoMaterias_Materias_IdMateria",
                table: "GrupoMaterias",
                column: "IdMateria",
                principalTable: "Materias",
                principalColumn: "IdMateria",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GrupoMaterias_Grupos_IdGrupo",
                table: "GrupoMaterias");

            migrationBuilder.DropForeignKey(
                name: "FK_GrupoMaterias_Materias_IdMateria",
                table: "GrupoMaterias");

            migrationBuilder.AddForeignKey(
                name: "FK_GrupoMaterias_Grupos_IdGrupo",
                table: "GrupoMaterias",
                column: "IdGrupo",
                principalTable: "Grupos",
                principalColumn: "IdGrupo",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GrupoMaterias_Materias_IdMateria",
                table: "GrupoMaterias",
                column: "IdMateria",
                principalTable: "Materias",
                principalColumn: "IdMateria",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
