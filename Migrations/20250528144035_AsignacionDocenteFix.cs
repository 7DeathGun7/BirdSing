using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BirdSing.Migrations
{
    /// <inheritdoc />
    public partial class AsignacionDocenteFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AsignacionDocentes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdDocente = table.Column<int>(type: "int", nullable: false),
                    IdMateria = table.Column<int>(type: "int", nullable: false),
                    IdGrupo = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsignacionDocentes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AsignacionDocentes_Docentes_IdDocente",
                        column: x => x.IdDocente,
                        principalTable: "Docentes",
                        principalColumn: "IdDocente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AsignacionDocentes_Grupos_IdGrupo",
                        column: x => x.IdGrupo,
                        principalTable: "Grupos",
                        principalColumn: "IdGrupo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AsignacionDocentes_Materias_IdMateria",
                        column: x => x.IdMateria,
                        principalTable: "Materias",
                        principalColumn: "IdMateria",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionDocentes_IdDocente",
                table: "AsignacionDocentes",
                column: "IdDocente");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionDocentes_IdGrupo",
                table: "AsignacionDocentes",
                column: "IdGrupo");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionDocentes_IdMateria",
                table: "AsignacionDocentes",
                column: "IdMateria");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AsignacionDocentes");
        }
    }
}
