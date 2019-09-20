using Microsoft.EntityFrameworkCore.Migrations;

namespace TreeDiagram.Migrations
{
    public partial class RoleRequest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ServerRoleRequestId",
                table: "UlongRoleId",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ServerRoleRequests",
                columns: table => new
                {
                    Id = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerRoleRequests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UlongRoleId_ServerRoleRequestId",
                table: "UlongRoleId",
                column: "ServerRoleRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_UlongRoleId_ServerRoleRequests_ServerRoleRequestId",
                table: "UlongRoleId",
                column: "ServerRoleRequestId",
                principalTable: "ServerRoleRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UlongRoleId_ServerRoleRequests_ServerRoleRequestId",
                table: "UlongRoleId");

            migrationBuilder.DropTable(
                name: "ServerRoleRequests");

            migrationBuilder.DropIndex(
                name: "IX_UlongRoleId_ServerRoleRequestId",
                table: "UlongRoleId");

            migrationBuilder.DropColumn(
                name: "ServerRoleRequestId",
                table: "UlongRoleId");
        }
    }
}
