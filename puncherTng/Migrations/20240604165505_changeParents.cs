using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace puncherTng.Migrations
{
    /// <inheritdoc />
    public partial class changeParents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "code_identification",
                table: "companie",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "id_companie",
                table: "agentes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "code_identification",
                table: "companie");

            migrationBuilder.DropColumn(
                name: "id_companie",
                table: "agentes");
        }
    }
}
