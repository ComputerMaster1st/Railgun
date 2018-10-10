using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace TreeDiagram.Migrations
{
    public partial class FinalDbPort : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FilterCapses",
                columns: table => new
                {
                    Id = table.Column<decimal>(nullable: false),
                    IsEnabled = table.Column<bool>(nullable: false),
                    IncludeBots = table.Column<bool>(nullable: false),
                    Percentage = table.Column<int>(nullable: false),
                    Length = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilterCapses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FilterUrls",
                columns: table => new
                {
                    Id = table.Column<decimal>(nullable: false),
                    IsEnabled = table.Column<bool>(nullable: false),
                    IncludeBots = table.Column<bool>(nullable: false),
                    BlockServerInvites = table.Column<bool>(nullable: false),
                    DenyMode = table.Column<bool>(nullable: false),
                    BannedUrls = table.Column<List<string>>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilterUrls", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FunBites",
                columns: table => new
                {
                    Id = table.Column<decimal>(nullable: false),
                    IsEnabled = table.Column<bool>(nullable: false),
                    Bites = table.Column<List<string>>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FunBites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FunRsts",
                columns: table => new
                {
                    Id = table.Column<decimal>(nullable: false),
                    IsEnabled = table.Column<bool>(nullable: false),
                    Rst = table.Column<List<string>>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FunRsts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerCommands",
                columns: table => new
                {
                    Id = table.Column<decimal>(nullable: false),
                    Prefix = table.Column<string>(nullable: true),
                    DeleteCmdAfterUse = table.Column<bool>(nullable: false),
                    RespondToBots = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerCommands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerJoinLeaves",
                columns: table => new
                {
                    Id = table.Column<decimal>(nullable: false),
                    SendToDM = table.Column<bool>(nullable: false),
                    ChannelId = table.Column<decimal>(nullable: false),
                    JoinMessages = table.Column<List<string>>(nullable: true),
                    LeaveMessages = table.Column<List<string>>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerJoinLeaves", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerMentions",
                columns: table => new
                {
                    Id = table.Column<decimal>(nullable: false),
                    DisableMentions = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerMentions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerMusics",
                columns: table => new
                {
                    Id = table.Column<decimal>(nullable: false),
                    AutoTextChannel = table.Column<decimal>(nullable: false),
                    AutoVoiceChannel = table.Column<decimal>(nullable: false),
                    AutoSkip = table.Column<bool>(nullable: false),
                    AutoDownload = table.Column<bool>(nullable: false),
                    PlaylistId = table.Column<string>(nullable: false),
                    VoteSkipEnabled = table.Column<bool>(nullable: false),
                    VoteSkipLimit = table.Column<int>(nullable: false),
                    SilentNowPlaying = table.Column<bool>(nullable: false),
                    SilentSongProcessing = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerMusics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerWarnings",
                columns: table => new
                {
                    Id = table.Column<decimal>(nullable: false),
                    WarnLimit = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerWarnings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TimerRemindMes",
                columns: table => new
                {
                    Id = table.Column<decimal>(nullable: false),
                    GuildId = table.Column<decimal>(nullable: false),
                    TextChannelId = table.Column<decimal>(nullable: false),
                    TimerExpire = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<decimal>(nullable: false),
                    Message = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimerRemindMes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserCommands",
                columns: table => new
                {
                    Id = table.Column<decimal>(nullable: false),
                    Prefix = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCommands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserMentions",
                columns: table => new
                {
                    Id = table.Column<decimal>(nullable: false),
                    DisableMentions = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMentions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IgnoredChannels",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ChannelId = table.Column<decimal>(nullable: false),
                    FilterCapsId = table.Column<decimal>(nullable: true),
                    FilterUrlId = table.Column<decimal>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IgnoredChannels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IgnoredChannels_FilterCapses_FilterCapsId",
                        column: x => x.FilterCapsId,
                        principalTable: "FilterCapses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IgnoredChannels_FilterUrls_FilterUrlId",
                        column: x => x.FilterUrlId,
                        principalTable: "FilterUrls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServerWarningInfo",
                columns: table => new
                {
                    WarningId = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    UserId = table.Column<decimal>(nullable: false),
                    Reasons = table.Column<List<string>>(nullable: true),
                    ServerWarningId = table.Column<decimal>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerWarningInfo", x => x.WarningId);
                    table.ForeignKey(
                        name: "FK_ServerWarningInfo_ServerWarnings_ServerWarningId",
                        column: x => x.ServerWarningId,
                        principalTable: "ServerWarnings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IgnoredChannels_FilterCapsId",
                table: "IgnoredChannels",
                column: "FilterCapsId");

            migrationBuilder.CreateIndex(
                name: "IX_IgnoredChannels_FilterUrlId",
                table: "IgnoredChannels",
                column: "FilterUrlId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerWarningInfo_ServerWarningId",
                table: "ServerWarningInfo",
                column: "ServerWarningId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FunBites");

            migrationBuilder.DropTable(
                name: "FunRsts");

            migrationBuilder.DropTable(
                name: "IgnoredChannels");

            migrationBuilder.DropTable(
                name: "ServerCommands");

            migrationBuilder.DropTable(
                name: "ServerJoinLeaves");

            migrationBuilder.DropTable(
                name: "ServerMentions");

            migrationBuilder.DropTable(
                name: "ServerMusics");

            migrationBuilder.DropTable(
                name: "ServerWarningInfo");

            migrationBuilder.DropTable(
                name: "TimerRemindMes");

            migrationBuilder.DropTable(
                name: "UserCommands");

            migrationBuilder.DropTable(
                name: "UserMentions");

            migrationBuilder.DropTable(
                name: "FilterCapses");

            migrationBuilder.DropTable(
                name: "FilterUrls");

            migrationBuilder.DropTable(
                name: "ServerWarnings");
        }
    }
}
