using Microsoft.EntityFrameworkCore.Migrations;

namespace TreeDiagram.Migrations
{
    public partial class AddedAutoLoop : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PlaylistAutoLoop",
                table: "ServerMusics",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlaylistAutoLoop",
                table: "ServerMusics");
        }
    }
}
