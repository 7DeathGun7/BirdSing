using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BirdSing.Migrations
{
    /// <inheritdoc />
    public partial class AddIdGradoToAsignacionDocentes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "gradoIdGrado",
                table: "AsignacionDocentes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionDocentes_gradoIdGrado",
                table: "AsignacionDocentes",
                column: "gradoIdGrado");

            migrationBuilder.AddForeignKey(
                name: "FK_AsignacionDocentes_Grados_gradoIdGrado",
                table: "AsignacionDocentes",
                column: "gradoIdGrado",
                principalTable: "Grados",
                principalColumn: "IdGrado",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AsignacionDocentes_Grados_gradoIdGrado",
                table: "AsignacionDocentes");

            migrationBuilder.DropIndex(
                name: "IX_AsignacionDocentes_gradoIdGrado",
                table: "AsignacionDocentes");

            migrationBuilder.DropColumn(
                name: "gradoIdGrado",
                table: "AsignacionDocentes");
        }
    }
}
