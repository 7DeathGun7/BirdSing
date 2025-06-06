using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BirdSing.Migrations
{
    /// <inheritdoc />
    public partial class MateriaPermiteGradoNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Materias_Grados_IdGrado",
                table: "Materias");

            migrationBuilder.AlterColumn<int>(
                name: "IdGrado",
                table: "Materias",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Materias_Grados_IdGrado",
                table: "Materias",
                column: "IdGrado",
                principalTable: "Grados",
                principalColumn: "IdGrado",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Materias_Grados_IdGrado",
                table: "Materias");

            migrationBuilder.AlterColumn<int>(
                name: "IdGrado",
                table: "Materias",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Materias_Grados_IdGrado",
                table: "Materias",
                column: "IdGrado",
                principalTable: "Grados",
                principalColumn: "IdGrado",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
