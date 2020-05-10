using Microsoft.EntityFrameworkCore.Migrations;

namespace TreeDiagram.Migrations
{
    public partial class IgnoreModifiedMessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IgnoreModifiedMessages",
                table: "ServerCommands",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IgnoreModifiedMessages",
                table: "ServerCommands");
        }
    }
}
