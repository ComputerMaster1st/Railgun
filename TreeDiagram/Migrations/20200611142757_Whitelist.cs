using Microsoft.EntityFrameworkCore.Migrations;

namespace TreeDiagram.Migrations
{
    public partial class Whitelist : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "WhitelistMode",
                table: "ServerMusics",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WhitelistMode",
                table: "ServerMusics");
        }
    }
}
