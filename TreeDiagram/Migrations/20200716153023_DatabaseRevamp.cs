using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace TreeDiagram.Migrations
{
    public partial class DatabaseRevamp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FilterCaps",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IsEnabled = table.Column<bool>(nullable: false),
                    IncludeBots = table.Column<bool>(nullable: false),
                    IgnoredChannels = table.Column<List<ulong>>(type: "jsonb", nullable: true),
                    Percentage = table.Column<int>(nullable: false),
                    Length = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilterCaps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FilterUrl",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IsEnabled = table.Column<bool>(nullable: false),
                    IncludeBots = table.Column<bool>(nullable: false),
                    IgnoredChannels = table.Column<List<ulong>>(type: "jsonb", nullable: true),
                    BlockServerInvites = table.Column<bool>(nullable: false),
                    DenyMode = table.Column<bool>(nullable: false),
                    BannedUrls = table.Column<List<string>>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilterUrl", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FunBite",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IsEnabled = table.Column<bool>(nullable: false),
                    Bites = table.Column<List<string>>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FunBite", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FunRst",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IsEnabled = table.Column<bool>(nullable: false),
                    Rst = table.Column<List<string>>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FunRst", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerCommand",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Prefix = table.Column<string>(nullable: true),
                    DeleteCmdAfterUse = table.Column<bool>(nullable: false),
                    RespondToBots = table.Column<bool>(nullable: false),
                    IgnoreModifiedMessages = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerCommand", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerGlobals",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DisableMentions = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerGlobals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerInactivity",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IsEnabled = table.Column<bool>(nullable: false),
                    InactiveDaysThreshold = table.Column<int>(nullable: false),
                    KickDaysThreshold = table.Column<int>(nullable: false),
                    InactiveRoleId = table.Column<decimal>(nullable: false),
                    UserWhitelist = table.Column<List<ulong>>(type: "jsonb", nullable: true),
                    RoleWhitelist = table.Column<List<ulong>>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerInactivity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerJoinLeave",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SendToDM = table.Column<bool>(nullable: false),
                    ChannelId = table.Column<decimal>(nullable: false),
                    DeleteAfterMinutes = table.Column<int>(nullable: false),
                    JoinMessages = table.Column<List<string>>(nullable: true),
                    LeaveMessages = table.Column<List<string>>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerJoinLeave", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerMusic",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AutoTextChannel = table.Column<decimal>(nullable: false),
                    AutoVoiceChannel = table.Column<decimal>(nullable: false),
                    AutoSkip = table.Column<bool>(nullable: false),
                    AutoDownload = table.Column<bool>(nullable: false),
                    AutoPlaySong = table.Column<string>(nullable: true),
                    PlaylistAutoLoop = table.Column<bool>(nullable: false),
                    PlaylistId = table.Column<string>(nullable: false),
                    VoteSkipEnabled = table.Column<bool>(nullable: false),
                    VoteSkipLimit = table.Column<int>(nullable: false),
                    SilentNowPlaying = table.Column<bool>(nullable: false),
                    SilentSongProcessing = table.Column<bool>(nullable: false),
                    NowPlayingChannel = table.Column<decimal>(nullable: false),
                    AllowedRoles = table.Column<List<ulong>>(type: "jsonb", nullable: true),
                    WhitelistMode = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerMusic", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerRoleRequest",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleIds = table.Column<List<ulong>>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerRoleRequest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerWarning",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WarnLimit = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerWarning", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TimerAssignRoles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<decimal>(nullable: false),
                    TimerExpire = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimerKickUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TimerRemindMes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<decimal>(nullable: false),
                    TimerExpire = table.Column<DateTime>(nullable: false),
                    TextChannelId = table.Column<decimal>(nullable: false),
                    UserId = table.Column<decimal>(nullable: false),
                    Message = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimerRemindMes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserGlobals",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Prefix = table.Column<string>(nullable: true),
                    DisableMentions = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGlobals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerFilters",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CapsId = table.Column<int>(nullable: true),
                    UrlsId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerFilters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServerFilters_FilterCaps_CapsId",
                        column: x => x.CapsId,
                        principalTable: "FilterCaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServerFilters_FilterUrl_UrlsId",
                        column: x => x.UrlsId,
                        principalTable: "FilterUrl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServerFun",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BitesId = table.Column<int>(nullable: true),
                    RstId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerFun", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServerFun_FunBite_BitesId",
                        column: x => x.BitesId,
                        principalTable: "FunBite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServerFun_FunRst_RstId",
                        column: x => x.RstId,
                        principalTable: "FunRst",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserActivityContainer",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<decimal>(nullable: false),
                    LastActive = table.Column<DateTime>(nullable: false),
                    ServerInactivityId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActivityContainer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserActivityContainer_ServerInactivity_ServerInactivityId",
                        column: x => x.ServerInactivityId,
                        principalTable: "ServerInactivity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServerWarningInfo",
                columns: table => new
                {
                    WarningId = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<decimal>(nullable: false),
                    Reasons = table.Column<List<string>>(nullable: true),
                    ServerWarningId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerWarningInfo", x => x.WarningId);
                    table.ForeignKey(
                        name: "FK_ServerWarningInfo_ServerWarning_ServerWarningId",
                        column: x => x.ServerWarningId,
                        principalTable: "ServerWarning",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<decimal>(nullable: false),
                    GlobalsId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProfiles_UserGlobals_GlobalsId",
                        column: x => x.GlobalsId,
                        principalTable: "UserGlobals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServerProfiles",
                columns: table => new
                {
                    Id = table.Column<decimal>(nullable: false),
                    FiltersId = table.Column<int>(nullable: true),
                    FunId = table.Column<int>(nullable: true),
                    CommandId = table.Column<int>(nullable: true),
                    GlobalsId = table.Column<int>(nullable: true),
                    InactivityId = table.Column<int>(nullable: true),
                    JoinLeaveId = table.Column<int>(nullable: true),
                    MusicId = table.Column<int>(nullable: true),
                    RoleRequestId = table.Column<int>(nullable: true),
                    WarningId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServerProfiles_ServerCommand_CommandId",
                        column: x => x.CommandId,
                        principalTable: "ServerCommand",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServerProfiles_ServerFilters_FiltersId",
                        column: x => x.FiltersId,
                        principalTable: "ServerFilters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServerProfiles_ServerFun_FunId",
                        column: x => x.FunId,
                        principalTable: "ServerFun",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServerProfiles_ServerGlobals_GlobalsId",
                        column: x => x.GlobalsId,
                        principalTable: "ServerGlobals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServerProfiles_ServerInactivity_InactivityId",
                        column: x => x.InactivityId,
                        principalTable: "ServerInactivity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServerProfiles_ServerJoinLeave_JoinLeaveId",
                        column: x => x.JoinLeaveId,
                        principalTable: "ServerJoinLeave",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServerProfiles_ServerMusic_MusicId",
                        column: x => x.MusicId,
                        principalTable: "ServerMusic",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServerProfiles_ServerRoleRequest_RoleRequestId",
                        column: x => x.RoleRequestId,
                        principalTable: "ServerRoleRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServerProfiles_ServerWarning_WarningId",
                        column: x => x.WarningId,
                        principalTable: "ServerWarning",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServerFilters_CapsId",
                table: "ServerFilters",
                column: "CapsId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerFilters_UrlsId",
                table: "ServerFilters",
                column: "UrlsId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerFun_BitesId",
                table: "ServerFun",
                column: "BitesId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerFun_RstId",
                table: "ServerFun",
                column: "RstId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerProfiles_CommandId",
                table: "ServerProfiles",
                column: "CommandId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerProfiles_FiltersId",
                table: "ServerProfiles",
                column: "FiltersId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerProfiles_FunId",
                table: "ServerProfiles",
                column: "FunId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerProfiles_GlobalsId",
                table: "ServerProfiles",
                column: "GlobalsId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerProfiles_InactivityId",
                table: "ServerProfiles",
                column: "InactivityId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerProfiles_JoinLeaveId",
                table: "ServerProfiles",
                column: "JoinLeaveId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerProfiles_MusicId",
                table: "ServerProfiles",
                column: "MusicId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerProfiles_RoleRequestId",
                table: "ServerProfiles",
                column: "RoleRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerProfiles_WarningId",
                table: "ServerProfiles",
                column: "WarningId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerWarningInfo_ServerWarningId",
                table: "ServerWarningInfo",
                column: "ServerWarningId");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityContainer_ServerInactivityId",
                table: "UserActivityContainer",
                column: "ServerInactivityId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_GlobalsId",
                table: "UserProfiles",
                column: "GlobalsId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServerProfiles");

            migrationBuilder.DropTable(
                name: "ServerWarningInfo");

            migrationBuilder.DropTable(
                name: "TimerAssignRoles");

            migrationBuilder.DropTable(
                name: "TimerKickUsers");

            migrationBuilder.DropTable(
                name: "TimerRemindMes");

            migrationBuilder.DropTable(
                name: "UserActivityContainer");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropTable(
                name: "ServerCommand");

            migrationBuilder.DropTable(
                name: "ServerFilters");

            migrationBuilder.DropTable(
                name: "ServerFun");

            migrationBuilder.DropTable(
                name: "ServerGlobals");

            migrationBuilder.DropTable(
                name: "ServerJoinLeave");

            migrationBuilder.DropTable(
                name: "ServerMusic");

            migrationBuilder.DropTable(
                name: "ServerRoleRequest");

            migrationBuilder.DropTable(
                name: "ServerWarning");

            migrationBuilder.DropTable(
                name: "ServerInactivity");

            migrationBuilder.DropTable(
                name: "UserGlobals");

            migrationBuilder.DropTable(
                name: "FilterCaps");

            migrationBuilder.DropTable(
                name: "FilterUrl");

            migrationBuilder.DropTable(
                name: "FunBite");

            migrationBuilder.DropTable(
                name: "FunRst");
        }
    }
}
