using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BirdSing.Migrations
{
    /// <inheritdoc />
    public partial class PendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GrupoMaterias_Materias_IdMateria",
                table: "GrupoMaterias");

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
                name: "FK_GrupoMaterias_Materias_IdMateria",
                table: "GrupoMaterias");

            migrationBuilder.AddForeignKey(
                name: "FK_GrupoMaterias_Materias_IdMateria",
                table: "GrupoMaterias",
                column: "IdMateria",
                principalTable: "Materias",
                principalColumn: "IdMateria",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
