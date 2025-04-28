using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BirdSing.Migrations
{
    /// <inheritdoc />
    public partial class AgregaDocenteGrupo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocentesGrupos",
                columns: table => new
                {
                    IdDocente = table.Column<int>(type: "int", nullable: false),
                    IdGrado = table.Column<int>(type: "int", nullable: false),
                    IdGrupo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocentesGrupos", x => new { x.IdDocente, x.IdGrado, x.IdGrupo });
                    table.ForeignKey(
                        name: "FK_DocentesGrupos_Docentes_IdDocente",
                        column: x => x.IdDocente,
                        principalTable: "Docentes",
                        principalColumn: "IdDocente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocentesGrupos_Grados_IdGrado",
                        column: x => x.IdGrado,
                        principalTable: "Grados",
                        principalColumn: "IdGrado",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocentesGrupos_Grupos_IdGrupo",
                        column: x => x.IdGrupo,
                        principalTable: "Grupos",
                        principalColumn: "IdGrupo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocentesGrupos_IdGrado",
                table: "DocentesGrupos",
                column: "IdGrado");

            migrationBuilder.CreateIndex(
                name: "IX_DocentesGrupos_IdGrupo",
                table: "DocentesGrupos",
                column: "IdGrupo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocentesGrupos");
        }
    }
}
