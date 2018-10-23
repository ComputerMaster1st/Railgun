using Microsoft.EntityFrameworkCore.Migrations;

namespace TreeDiagram.Migrations
{
    public partial class JoinLeave_DeleteAfter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeleteAfterMinutes",
                table: "ServerJoinLeaves",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeleteAfterMinutes",
                table: "ServerJoinLeaves");
        }
    }
}
