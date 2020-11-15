using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace TreeDiagram.Migrations
{
    public partial class MusicAutoJoinConfig_ID_Added : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MusicAutoJoinConfig",
                table: "MusicAutoJoinConfig");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "MusicAutoJoinConfig",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MusicAutoJoinConfig",
                table: "MusicAutoJoinConfig",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MusicAutoJoinConfig",
                table: "MusicAutoJoinConfig");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "MusicAutoJoinConfig");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MusicAutoJoinConfig",
                table: "MusicAutoJoinConfig",
                column: "VoiceChannelId");
        }
    }
}
