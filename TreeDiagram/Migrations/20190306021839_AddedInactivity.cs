using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace TreeDiagram.Migrations
{
    public partial class AddedInactivity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AllowedRole");

            migrationBuilder.CreateTable(
                name: "ServerInactivities",
                columns: table => new
                {
                    Id = table.Column<decimal>(nullable: false),
                    IsEnabled = table.Column<bool>(nullable: false),
                    InactiveDaysThreshold = table.Column<int>(nullable: false),
                    KickDaysThreshold = table.Column<int>(nullable: false),
                    InactiveRoleId = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerInactivities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TimerAssignRoles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    GuildId = table.Column<decimal>(nullable: false),
                    TimerExpire = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<decimal>(nullable: false),
                    RoleId = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimerAssignRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TimerKickUsers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    GuildId = table.Column<decimal>(nullable: false),
                    TimerExpire = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimerKickUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UlongRoleId",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    RoleId = table.Column<decimal>(nullable: false),
                    ServerInactivityId = table.Column<decimal>(nullable: true),
                    ServerMusicId = table.Column<decimal>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UlongRoleId", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UlongRoleId_ServerInactivities_ServerInactivityId",
                        column: x => x.ServerInactivityId,
                        principalTable: "ServerInactivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UlongRoleId_ServerMusics_ServerMusicId",
                        column: x => x.ServerMusicId,
                        principalTable: "ServerMusics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UlongUserId",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    UserId = table.Column<decimal>(nullable: false),
                    ServerInactivityId = table.Column<decimal>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UlongUserId", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UlongUserId_ServerInactivities_ServerInactivityId",
                        column: x => x.ServerInactivityId,
                        principalTable: "ServerInactivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserActivityContainer",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    UserId = table.Column<decimal>(nullable: false),
                    LastActive = table.Column<DateTime>(nullable: false),
                    ServerInactivityId = table.Column<decimal>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActivityContainer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserActivityContainer_ServerInactivities_ServerInactivityId",
                        column: x => x.ServerInactivityId,
                        principalTable: "ServerInactivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UlongRoleId_ServerInactivityId",
                table: "UlongRoleId",
                column: "ServerInactivityId");

            migrationBuilder.CreateIndex(
                name: "IX_UlongRoleId_ServerMusicId",
                table: "UlongRoleId",
                column: "ServerMusicId");

            migrationBuilder.CreateIndex(
                name: "IX_UlongUserId_ServerInactivityId",
                table: "UlongUserId",
                column: "ServerInactivityId");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityContainer_ServerInactivityId",
                table: "UserActivityContainer",
                column: "ServerInactivityId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimerAssignRoles");

            migrationBuilder.DropTable(
                name: "TimerKickUsers");

            migrationBuilder.DropTable(
                name: "UlongRoleId");

            migrationBuilder.DropTable(
                name: "UlongUserId");

            migrationBuilder.DropTable(
                name: "UserActivityContainer");

            migrationBuilder.DropTable(
                name: "ServerInactivities");

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
    }
}
