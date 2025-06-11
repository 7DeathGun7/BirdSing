using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BirdSing.Migrations
{
    /// <inheritdoc />
    public partial class ChangeSaldoToDecimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AlumnoMatriculaAlumno",
                table: "AlumnosTutores",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CuentaPausada",
                table: "Alumnos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Saldo",
                table: "Alumnos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "CompraTutores",
                columns: table => new
                {
                    IdCompraTutor = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdAlumno = table.Column<int>(type: "int", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompraTutores", x => x.IdCompraTutor);
                    table.ForeignKey(
                        name: "FK_CompraTutores_Alumnos_IdAlumno",
                        column: x => x.IdAlumno,
                        principalTable: "Alumnos",
                        principalColumn: "MatriculaAlumno",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlumnosTutores_AlumnoMatriculaAlumno",
                table: "AlumnosTutores",
                column: "AlumnoMatriculaAlumno");

            migrationBuilder.CreateIndex(
                name: "IX_CompraTutores_IdAlumno",
                table: "CompraTutores",
                column: "IdAlumno");

            migrationBuilder.AddForeignKey(
                name: "FK_AlumnosTutores_Alumnos_AlumnoMatriculaAlumno",
                table: "AlumnosTutores",
                column: "AlumnoMatriculaAlumno",
                principalTable: "Alumnos",
                principalColumn: "MatriculaAlumno");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlumnosTutores_Alumnos_AlumnoMatriculaAlumno",
                table: "AlumnosTutores");

            migrationBuilder.DropTable(
                name: "CompraTutores");

            migrationBuilder.DropIndex(
                name: "IX_AlumnosTutores_AlumnoMatriculaAlumno",
                table: "AlumnosTutores");

            migrationBuilder.DropColumn(
                name: "AlumnoMatriculaAlumno",
                table: "AlumnosTutores");

            migrationBuilder.DropColumn(
                name: "CuentaPausada",
                table: "Alumnos");

            migrationBuilder.DropColumn(
                name: "Saldo",
                table: "Alumnos");
        }
    }
}
