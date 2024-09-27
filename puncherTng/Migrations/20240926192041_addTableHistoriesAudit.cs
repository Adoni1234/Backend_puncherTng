using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace puncherTng.Migrations
{
    /// <inheritdoc />
    public partial class addTableHistoriesAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "history_audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdmissionDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExitDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdAngente = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_history_audit", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "history_audit");
        }
    }
}
