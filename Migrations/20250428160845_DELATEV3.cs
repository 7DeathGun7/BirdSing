using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BirdSing.Migrations
{
    /// <inheritdoc />
    public partial class DELATEV3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Grupos",
                table: "Grupos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "GrupoIdGrupo",
                table: "DocentesGrupos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocentesGrupos_GrupoIdGrupo",
                table: "DocentesGrupos",
                column: "GrupoIdGrupo");

            migrationBuilder.AddForeignKey(
                name: "FK_DocentesGrupos_Grupos_GrupoIdGrupo",
                table: "DocentesGrupos",
                column: "GrupoIdGrupo",
                principalTable: "Grupos",
                principalColumn: "IdGrupo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocentesGrupos_Grupos_GrupoIdGrupo",
                table: "DocentesGrupos");

            migrationBuilder.DropIndex(
                name: "IX_DocentesGrupos_GrupoIdGrupo",
                table: "DocentesGrupos");

            migrationBuilder.DropColumn(
                name: "GrupoIdGrupo",
                table: "DocentesGrupos");

            migrationBuilder.AlterColumn<string>(
                name: "Grupos",
                table: "Grupos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}
