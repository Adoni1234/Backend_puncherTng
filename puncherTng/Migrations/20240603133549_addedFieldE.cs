using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace puncherTng.Migrations
{
    /// <inheritdoc />
    public partial class addedFieldE : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Designatio",
                table: "agentes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Designatio",
                table: "agentes");
        }
    }
}
