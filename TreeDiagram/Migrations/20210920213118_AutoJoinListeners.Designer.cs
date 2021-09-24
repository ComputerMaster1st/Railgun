﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TreeDiagram;

namespace TreeDiagram.Migrations
{
    [DbContext(typeof(TreeDiagramContext))]
    [Migration("20210920213118_AutoJoinListeners")]
    partial class AutoJoinListeners
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("TreeDiagram.Models.Filter.FilterCaps", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<long[]>("IgnoredChannels")
                        .HasColumnType("bigint[]");

                    b.Property<bool>("IncludeBots")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("boolean");

                    b.Property<int>("Length")
                        .HasColumnType("integer");

                    b.Property<int>("Percentage")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("FilterCaps");
                });

            modelBuilder.Entity("TreeDiagram.Models.Filter.FilterUrl", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<List<string>>("BannedUrls")
                        .HasColumnType("text[]");

                    b.Property<bool>("BlockServerInvites")
                        .HasColumnType("boolean");

                    b.Property<bool>("DenyMode")
                        .HasColumnType("boolean");

                    b.Property<long[]>("IgnoredChannels")
                        .HasColumnType("bigint[]");

                    b.Property<bool>("IncludeBots")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.ToTable("FilterUrl");
                });

            modelBuilder.Entity("TreeDiagram.Models.Filter.ServerFilters", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int?>("CapsId")
                        .HasColumnType("integer");

                    b.Property<int?>("UrlsId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CapsId");

                    b.HasIndex("UrlsId");

                    b.ToTable("ServerFilters");
                });

            modelBuilder.Entity("TreeDiagram.Models.Fun.FunBite", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<List<string>>("Bites")
                        .HasColumnType("text[]");

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("boolean");

                    b.Property<bool>("NoNameRandomness")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.ToTable("FunBite");
                });

            modelBuilder.Entity("TreeDiagram.Models.Fun.FunRst", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("boolean");

                    b.Property<List<string>>("Rst")
                        .HasColumnType("text[]");

                    b.HasKey("Id");

                    b.ToTable("FunRst");
                });

            modelBuilder.Entity("TreeDiagram.Models.Fun.ServerFun", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int?>("BitesId")
                        .HasColumnType("integer");

                    b.Property<int?>("RstId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("BitesId");

                    b.HasIndex("RstId");

                    b.ToTable("ServerFun");
                });

            modelBuilder.Entity("TreeDiagram.Models.Server.ServerCommand", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<bool>("DeleteCmdAfterUse")
                        .HasColumnType("boolean");

                    b.Property<bool>("IgnoreModifiedMessages")
                        .HasColumnType("boolean");

                    b.Property<string>("Prefix")
                        .HasColumnType("text");

                    b.Property<bool>("RespondToBots")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.ToTable("ServerCommand");
                });

            modelBuilder.Entity("TreeDiagram.Models.Server.ServerGlobals", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<bool>("DisableMentions")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.ToTable("ServerGlobals");
                });

            modelBuilder.Entity("TreeDiagram.Models.Server.ServerInactivity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("InactiveDaysThreshold")
                        .HasColumnType("integer");

                    b.Property<decimal>("InactiveRoleId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("boolean");

                    b.Property<int>("KickDaysThreshold")
                        .HasColumnType("integer");

                    b.Property<long[]>("RoleWhitelist")
                        .HasColumnType("bigint[]");

                    b.Property<long[]>("UserWhitelist")
                        .HasColumnType("bigint[]");

                    b.HasKey("Id");

                    b.ToTable("ServerInactivity");
                });

            modelBuilder.Entity("TreeDiagram.Models.Server.ServerJoinLeave", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int>("DeleteAfterMinutes")
                        .HasColumnType("integer");

                    b.Property<List<string>>("JoinMessages")
                        .HasColumnType("text[]");

                    b.Property<List<string>>("LeaveMessages")
                        .HasColumnType("text[]");

                    b.Property<bool>("SendToDM")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.ToTable("ServerJoinLeave");
                });

            modelBuilder.Entity("TreeDiagram.Models.Server.ServerMusic", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<long[]>("AllowedRoles")
                        .HasColumnType("bigint[]");

                    b.Property<bool>("AutoDownload")
                        .HasColumnType("boolean");

                    b.Property<string>("AutoPlaySong")
                        .HasColumnType("text");

                    b.Property<bool>("AutoSkip")
                        .HasColumnType("boolean");

                    b.Property<bool>("DisableShuffle")
                        .HasColumnType("boolean");

                    b.Property<decimal>("NowPlayingChannel")
                        .HasColumnType("numeric(20,0)");

                    b.Property<bool>("PlaylistAutoLoop")
                        .HasColumnType("boolean");

                    b.Property<string>("PlaylistId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("SilentNowPlaying")
                        .HasColumnType("boolean");

                    b.Property<bool>("SilentSongProcessing")
                        .HasColumnType("boolean");

                    b.Property<bool>("VoteSkipEnabled")
                        .HasColumnType("boolean");

                    b.Property<int>("VoteSkipLimit")
                        .HasColumnType("integer");

                    b.Property<bool>("WhitelistMode")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.ToTable("ServerMusic");
                });

            modelBuilder.Entity("TreeDiagram.Models.Server.ServerRoleRequest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<long[]>("RoleIds")
                        .HasColumnType("bigint[]");

                    b.HasKey("Id");

                    b.ToTable("ServerRoleRequest");
                });

            modelBuilder.Entity("TreeDiagram.Models.Server.ServerWarning", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("WarnLimit")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("ServerWarning");
                });

            modelBuilder.Entity("TreeDiagram.Models.ServerProfile", b =>
                {
                    b.Property<decimal>("Id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int?>("CommandId")
                        .HasColumnType("integer");

                    b.Property<int?>("FiltersId")
                        .HasColumnType("integer");

                    b.Property<int?>("FunId")
                        .HasColumnType("integer");

                    b.Property<int?>("GlobalsId")
                        .HasColumnType("integer");

                    b.Property<int?>("InactivityId")
                        .HasColumnType("integer");

                    b.Property<int?>("JoinLeaveId")
                        .HasColumnType("integer");

                    b.Property<int?>("MusicId")
                        .HasColumnType("integer");

                    b.Property<int?>("RoleRequestId")
                        .HasColumnType("integer");

                    b.Property<int?>("WarningId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CommandId");

                    b.HasIndex("FiltersId");

                    b.HasIndex("FunId");

                    b.HasIndex("GlobalsId");

                    b.HasIndex("InactivityId");

                    b.HasIndex("JoinLeaveId");

                    b.HasIndex("MusicId");

                    b.HasIndex("RoleRequestId");

                    b.HasIndex("WarningId");

                    b.ToTable("ServerProfiles");
                });

            modelBuilder.Entity("TreeDiagram.Models.SubModels.MusicAutoJoinConfig", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<long[]>("ListenForUsers")
                        .HasColumnType("bigint[]");

                    b.Property<int?>("ServerMusicId")
                        .HasColumnType("integer");

                    b.Property<decimal>("TextChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("VoiceChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("ServerMusicId");

                    b.ToTable("MusicAutoJoinConfig");
                });

            modelBuilder.Entity("TreeDiagram.Models.SubModels.ServerWarningInfo", b =>
                {
                    b.Property<int>("WarningId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<List<string>>("Reasons")
                        .HasColumnType("text[]");

                    b.Property<int?>("ServerWarningId")
                        .HasColumnType("integer");

                    b.Property<decimal>("UserId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("WarningId");

                    b.HasIndex("ServerWarningId");

                    b.ToTable("ServerWarningInfo");
                });

            modelBuilder.Entity("TreeDiagram.Models.SubModels.UserActivityContainer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<DateTime>("LastActive")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int?>("ServerInactivityId")
                        .HasColumnType("integer");

                    b.Property<decimal>("UserId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("ServerInactivityId");

                    b.ToTable("UserActivityContainer");
                });

            modelBuilder.Entity("TreeDiagram.Models.TreeTimer.TimerAssignRole", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("RoleId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<DateTime>("TimerExpire")
                        .HasColumnType("timestamp without time zone");

                    b.Property<decimal>("UserId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.ToTable("TimerAssignRoles");
                });

            modelBuilder.Entity("TreeDiagram.Models.TreeTimer.TimerKickUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<DateTime>("TimerExpire")
                        .HasColumnType("timestamp without time zone");

                    b.Property<decimal>("UserId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.ToTable("TimerKickUsers");
                });

            modelBuilder.Entity("TreeDiagram.Models.TreeTimer.TimerRemindMe", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Message")
                        .HasColumnType("text");

                    b.Property<decimal>("TextChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<DateTime>("TimerExpire")
                        .HasColumnType("timestamp without time zone");

                    b.Property<decimal>("UserId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.ToTable("TimerRemindMes");
                });

            modelBuilder.Entity("TreeDiagram.Models.User.UserGlobals", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<bool>("DisableMentions")
                        .HasColumnType("boolean");

                    b.Property<string>("Prefix")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("UserGlobals");
                });

            modelBuilder.Entity("TreeDiagram.Models.UserProfile", b =>
                {
                    b.Property<decimal>("Id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int?>("GlobalsId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("GlobalsId");

                    b.ToTable("UserProfiles");
                });

            modelBuilder.Entity("TreeDiagram.Models.Filter.ServerFilters", b =>
                {
                    b.HasOne("TreeDiagram.Models.Filter.FilterCaps", "Caps")
                        .WithMany()
                        .HasForeignKey("CapsId");

                    b.HasOne("TreeDiagram.Models.Filter.FilterUrl", "Urls")
                        .WithMany()
                        .HasForeignKey("UrlsId");
                });

            modelBuilder.Entity("TreeDiagram.Models.Fun.ServerFun", b =>
                {
                    b.HasOne("TreeDiagram.Models.Fun.FunBite", "Bites")
                        .WithMany()
                        .HasForeignKey("BitesId");

                    b.HasOne("TreeDiagram.Models.Fun.FunRst", "Rst")
                        .WithMany()
                        .HasForeignKey("RstId");
                });

            modelBuilder.Entity("TreeDiagram.Models.ServerProfile", b =>
                {
                    b.HasOne("TreeDiagram.Models.Server.ServerCommand", "Command")
                        .WithMany()
                        .HasForeignKey("CommandId");

                    b.HasOne("TreeDiagram.Models.Filter.ServerFilters", "Filters")
                        .WithMany()
                        .HasForeignKey("FiltersId");

                    b.HasOne("TreeDiagram.Models.Fun.ServerFun", "Fun")
                        .WithMany()
                        .HasForeignKey("FunId");

                    b.HasOne("TreeDiagram.Models.Server.ServerGlobals", "Globals")
                        .WithMany()
                        .HasForeignKey("GlobalsId");

                    b.HasOne("TreeDiagram.Models.Server.ServerInactivity", "Inactivity")
                        .WithMany()
                        .HasForeignKey("InactivityId");

                    b.HasOne("TreeDiagram.Models.Server.ServerJoinLeave", "JoinLeave")
                        .WithMany()
                        .HasForeignKey("JoinLeaveId");

                    b.HasOne("TreeDiagram.Models.Server.ServerMusic", "Music")
                        .WithMany()
                        .HasForeignKey("MusicId");

                    b.HasOne("TreeDiagram.Models.Server.ServerRoleRequest", "RoleRequest")
                        .WithMany()
                        .HasForeignKey("RoleRequestId");

                    b.HasOne("TreeDiagram.Models.Server.ServerWarning", "Warning")
                        .WithMany()
                        .HasForeignKey("WarningId");
                });

            modelBuilder.Entity("TreeDiagram.Models.SubModels.MusicAutoJoinConfig", b =>
                {
                    b.HasOne("TreeDiagram.Models.Server.ServerMusic", null)
                        .WithMany("AutoJoinConfigs")
                        .HasForeignKey("ServerMusicId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TreeDiagram.Models.SubModels.ServerWarningInfo", b =>
                {
                    b.HasOne("TreeDiagram.Models.Server.ServerWarning", null)
                        .WithMany("Warnings")
                        .HasForeignKey("ServerWarningId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TreeDiagram.Models.SubModels.UserActivityContainer", b =>
                {
                    b.HasOne("TreeDiagram.Models.Server.ServerInactivity", null)
                        .WithMany("Users")
                        .HasForeignKey("ServerInactivityId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TreeDiagram.Models.UserProfile", b =>
                {
                    b.HasOne("TreeDiagram.Models.User.UserGlobals", "Globals")
                        .WithMany()
                        .HasForeignKey("GlobalsId");
                });
#pragma warning restore 612, 618
        }
    }
}
