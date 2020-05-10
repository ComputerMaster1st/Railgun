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
    [Migration("20200510004134_IgnoreModifiedMessages")]
    partial class IgnoreModifiedMessages
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
                    b.Property<decimal>("Id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<bool>("IncludeBots")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("boolean");

                    b.Property<int>("Length")
                        .HasColumnType("integer");

                    b.Property<int>("Percentage")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("FilterCapses");
                });

            modelBuilder.Entity("TreeDiagram.Models.Filter.FilterUrl", b =>
                {
                    b.Property<decimal>("Id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<List<string>>("BannedUrls")
                        .HasColumnType("text[]");

                    b.Property<bool>("BlockServerInvites")
                        .HasColumnType("boolean");

                    b.Property<bool>("DenyMode")
                        .HasColumnType("boolean");

                    b.Property<bool>("IncludeBots")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.ToTable("FilterUrls");
                });

            modelBuilder.Entity("TreeDiagram.Models.Fun.FunBite", b =>
                {
                    b.Property<decimal>("Id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<List<string>>("Bites")
                        .HasColumnType("text[]");

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.ToTable("FunBites");
                });

            modelBuilder.Entity("TreeDiagram.Models.Fun.FunRst", b =>
                {
                    b.Property<decimal>("Id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("boolean");

                    b.Property<List<string>>("Rst")
                        .HasColumnType("text[]");

                    b.HasKey("Id");

                    b.ToTable("FunRsts");
                });

            modelBuilder.Entity("TreeDiagram.Models.Server.ServerCommand", b =>
                {
                    b.Property<decimal>("Id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<bool>("DeleteCmdAfterUse")
                        .HasColumnType("boolean");

                    b.Property<bool>("IgnoreModifiedMessages")
                        .HasColumnType("boolean");

                    b.Property<string>("Prefix")
                        .HasColumnType("text");

                    b.Property<bool>("RespondToBots")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.ToTable("ServerCommands");
                });

            modelBuilder.Entity("TreeDiagram.Models.Server.ServerInactivity", b =>
                {
                    b.Property<decimal>("Id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int>("InactiveDaysThreshold")
                        .HasColumnType("integer");

                    b.Property<decimal>("InactiveRoleId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("boolean");

                    b.Property<int>("KickDaysThreshold")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("ServerInactivities");
                });

            modelBuilder.Entity("TreeDiagram.Models.Server.ServerJoinLeave", b =>
                {
                    b.Property<decimal>("Id")
                        .HasColumnType("numeric(20,0)");

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

                    b.ToTable("ServerJoinLeaves");
                });

            modelBuilder.Entity("TreeDiagram.Models.Server.ServerMention", b =>
                {
                    b.Property<decimal>("Id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<bool>("DisableMentions")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.ToTable("ServerMentions");
                });

            modelBuilder.Entity("TreeDiagram.Models.Server.ServerMusic", b =>
                {
                    b.Property<decimal>("Id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<bool>("AutoDownload")
                        .HasColumnType("boolean");

                    b.Property<string>("AutoPlaySong")
                        .HasColumnType("text");

                    b.Property<bool>("AutoSkip")
                        .HasColumnType("boolean");

                    b.Property<decimal>("AutoTextChannel")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("AutoVoiceChannel")
                        .HasColumnType("numeric(20,0)");

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

                    b.HasKey("Id");

                    b.ToTable("ServerMusics");
                });

            modelBuilder.Entity("TreeDiagram.Models.Server.ServerRoleRequest", b =>
                {
                    b.Property<decimal>("Id")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.ToTable("ServerRoleRequests");
                });

            modelBuilder.Entity("TreeDiagram.Models.Server.ServerWarning", b =>
                {
                    b.Property<decimal>("Id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int>("WarnLimit")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("ServerWarnings");
                });

            modelBuilder.Entity("TreeDiagram.Models.SubModels.IgnoredChannels", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal?>("FilterCapsId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal?>("FilterUrlId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("FilterCapsId");

                    b.HasIndex("FilterUrlId");

                    b.ToTable("IgnoredChannels");
                });

            modelBuilder.Entity("TreeDiagram.Models.SubModels.ServerWarningInfo", b =>
                {
                    b.Property<int>("WarningId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<List<string>>("Reasons")
                        .HasColumnType("text[]");

                    b.Property<decimal?>("ServerWarningId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("UserId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("WarningId");

                    b.HasIndex("ServerWarningId");

                    b.ToTable("ServerWarningInfo");
                });

            modelBuilder.Entity("TreeDiagram.Models.SubModels.UlongRoleId", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<decimal>("RoleId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal?>("ServerInactivityId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal?>("ServerMusicId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal?>("ServerRoleRequestId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("ServerInactivityId");

                    b.HasIndex("ServerMusicId");

                    b.HasIndex("ServerRoleRequestId");

                    b.ToTable("UlongRoleId");
                });

            modelBuilder.Entity("TreeDiagram.Models.SubModels.UlongUserId", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<decimal?>("ServerInactivityId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("UserId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("ServerInactivityId");

                    b.ToTable("UlongUserId");
                });

            modelBuilder.Entity("TreeDiagram.Models.SubModels.UserActivityContainer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<DateTime>("LastActive")
                        .HasColumnType("timestamp without time zone");

                    b.Property<decimal?>("ServerInactivityId")
                        .HasColumnType("numeric(20,0)");

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

            modelBuilder.Entity("TreeDiagram.Models.User.UserCommand", b =>
                {
                    b.Property<decimal>("Id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Prefix")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("UserCommands");
                });

            modelBuilder.Entity("TreeDiagram.Models.User.UserMention", b =>
                {
                    b.Property<decimal>("Id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<bool>("DisableMentions")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.ToTable("UserMentions");
                });

            modelBuilder.Entity("TreeDiagram.Models.SubModels.IgnoredChannels", b =>
                {
                    b.HasOne("TreeDiagram.Models.Filter.FilterCaps", null)
                        .WithMany("IgnoredChannels")
                        .HasForeignKey("FilterCapsId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("TreeDiagram.Models.Filter.FilterUrl", null)
                        .WithMany("IgnoredChannels")
                        .HasForeignKey("FilterUrlId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TreeDiagram.Models.SubModels.ServerWarningInfo", b =>
                {
                    b.HasOne("TreeDiagram.Models.Server.ServerWarning", null)
                        .WithMany("Warnings")
                        .HasForeignKey("ServerWarningId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TreeDiagram.Models.SubModels.UlongRoleId", b =>
                {
                    b.HasOne("TreeDiagram.Models.Server.ServerInactivity", null)
                        .WithMany("RoleWhitelist")
                        .HasForeignKey("ServerInactivityId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("TreeDiagram.Models.Server.ServerMusic", null)
                        .WithMany("AllowedRoles")
                        .HasForeignKey("ServerMusicId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("TreeDiagram.Models.Server.ServerRoleRequest", null)
                        .WithMany("RoleIds")
                        .HasForeignKey("ServerRoleRequestId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TreeDiagram.Models.SubModels.UlongUserId", b =>
                {
                    b.HasOne("TreeDiagram.Models.Server.ServerInactivity", null)
                        .WithMany("UserWhitelist")
                        .HasForeignKey("ServerInactivityId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TreeDiagram.Models.SubModels.UserActivityContainer", b =>
                {
                    b.HasOne("TreeDiagram.Models.Server.ServerInactivity", null)
                        .WithMany("Users")
                        .HasForeignKey("ServerInactivityId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
