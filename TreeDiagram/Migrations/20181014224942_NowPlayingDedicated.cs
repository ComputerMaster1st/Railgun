using Microsoft.EntityFrameworkCore.Migrations;

namespace TreeDiagram.Migrations
{
    public partial class NowPlayingDedicated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "NowPlayingChannel",
                table: "ServerMusics",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NowPlayingChannel",
                table: "ServerMusics");
        }
    }
}
