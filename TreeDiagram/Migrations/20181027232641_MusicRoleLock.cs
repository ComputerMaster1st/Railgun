using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace TreeDiagram.Migrations
{
    public partial class MusicRoleLock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AllowedRole",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    RoleId = table.Column<decimal>(nullable: false),
                    ServerMusicId = table.Column<decimal>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllowedRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AllowedRole_ServerMusics_ServerMusicId",
                        column: x => x.ServerMusicId,
                        principalTable: "ServerMusics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AllowedRole_ServerMusicId",
                table: "AllowedRole",
                column: "ServerMusicId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AllowedRole");
        }
    }
}
