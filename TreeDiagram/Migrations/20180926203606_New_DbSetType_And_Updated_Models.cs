﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace TreeDiagram.Migrations
{
    public partial class New_DbSetType_And_Updated_Models : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FilterCapses");

            migrationBuilder.DropTable(
                name: "FilterUrls");

            migrationBuilder.DropTable(
                name: "FunBites");

            migrationBuilder.DropTable(
                name: "FunRsts");

            migrationBuilder.DropTable(
                name: "ServerCommands");

            migrationBuilder.DropTable(
                name: "ServerJoinLeaves");

            migrationBuilder.DropTable(
                name: "ServerMentions");

            migrationBuilder.DropTable(
                name: "ServerWarningInfo");

            migrationBuilder.DropTable(
                name: "TimerRemindMes");

            migrationBuilder.DropTable(
                name: "UserCommands");

            migrationBuilder.DropTable(
                name: "UserMentions");

            migrationBuilder.DropTable(
                name: "ServerWarnings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServerMusics",
                table: "ServerMusics");

            migrationBuilder.RenameTable(
                name: "ServerMusics",
                newName: "ServerMusic");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServerMusic",
                table: "ServerMusic",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ServerMusic",
                table: "ServerMusic");

            migrationBuilder.RenameTable(
                name: "ServerMusic",
                newName: "ServerMusics");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServerMusics",
                table: "ServerMusics",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "FilterCapses",
                columns: table => new
                {
                    Id = table.Column<decimal>(nullable: false),
                    IgnoredChannels = table.Column<List<ulong>>(nullable: true),
                    IncludeBots = table.Column<bool>(nullable: false),
                    IsEnabled = table.Column<bool>(nullable: false),
                    Length = table.Column<int>(nullable: false),
                    Percentage = table.Column<int>(nullable: false)
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
                    BannedUrls = table.Column<List<string>>(nullable: true),
                    BlockServerInvites = table.Column<bool>(nullable: false),
                    DenyMode = table.Column<bool>(nullable: false),
                    IgnoredChannels = table.Column<List<ulong>>(nullable: true),
                    IncludeBots = table.Column<bool>(nullable: false),
                    IsEnabled = table.Column<bool>(nullable: false)
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
                    Bites = table.Column<List<string>>(nullable: true),
                    IsEnabled = table.Column<bool>(nullable: false)
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
                    DeleteCmdAfterUse = table.Column<bool>(nullable: false),
                    Prefix = table.Column<string>(nullable: true),
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
                    ChannelId = table.Column<decimal>(nullable: false),
                    JoinMessages = table.Column<List<string>>(nullable: true),
                    LeaveMessages = table.Column<List<string>>(nullable: true),
                    SendToDM = table.Column<bool>(nullable: false)
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
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    GuildId = table.Column<decimal>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    TextChannelId = table.Column<decimal>(nullable: false),
                    TimerExpire = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<decimal>(nullable: false)
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
                name: "ServerWarningInfo",
                columns: table => new
                {
                    UserId = table.Column<decimal>(nullable: false),
                    Reasons = table.Column<List<string>>(nullable: true),
                    ServerWarningId = table.Column<decimal>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerWarningInfo", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_ServerWarningInfo_ServerWarnings_ServerWarningId",
                        column: x => x.ServerWarningId,
                        principalTable: "ServerWarnings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServerWarningInfo_ServerWarningId",
                table: "ServerWarningInfo",
                column: "ServerWarningId");
        }
    }
}
