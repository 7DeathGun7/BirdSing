using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BirdSing.Migrations
{
    /// <inheritdoc />
    public partial class AddActivoToEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Usuarios",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Direccion",
                table: "Tutores",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Tutores",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "MateriasDocentes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Materias",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Grupos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "GrupoMaterias",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Grados",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "DocentesGrupos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Docentes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Avisos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "AlumnosTutores",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Alumnos",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Tutores");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "MateriasDocentes");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Materias");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Grupos");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "GrupoMaterias");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Grados");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "DocentesGrupos");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Docentes");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Avisos");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "AlumnosTutores");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Alumnos");

            migrationBuilder.AlterColumn<string>(
                name: "Direccion",
                table: "Tutores",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);
        }
    }
}
