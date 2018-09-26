﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TreeDiagram;

namespace TreeDiagram.Migrations
{
    [DbContext(typeof(TreeDiagramContext))]
    [Migration("20180926203606_New_DbSetType_And_Updated_Models")]
    partial class New_DbSetType_And_Updated_Models
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.1.3-rtm-32065")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("TreeDiagram.Models.Server.ServerMusic", b =>
                {
                    b.Property<decimal>("Id")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<bool>("AutoDownload");

                    b.Property<bool>("AutoSkip");

                    b.Property<decimal>("AutoTextChannel")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<decimal>("AutoVoiceChannel")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<string>("PlaylistId")
                        .IsRequired();

                    b.Property<bool>("SilentNowPlaying");

                    b.Property<bool>("SilentSongProcessing");

                    b.Property<bool>("VoteSkipEnabled");

                    b.Property<int>("VoteSkipLimit");

                    b.HasKey("Id");

                    b.ToTable("ServerMusic");
                });
#pragma warning restore 612, 618
        }
    }
}
