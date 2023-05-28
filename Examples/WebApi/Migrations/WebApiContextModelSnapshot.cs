﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WebApi.Data;

#nullable disable

namespace WebApi.Migrations
{
    [DbContext(typeof(WebApiContext))]
    partial class WebApiContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.15")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("WebApi.Entities.OtherFile", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("ChunkBytesLength")
                        .HasColumnType("int");

                    b.Property<byte[]>("Data")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<Guid>("FileId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("MimeType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("NextId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("TimeStamp")
                        .HasColumnType("datetimeoffset");

                    b.Property<long>("TotalBytesLength")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("OtherFile");
                });

            modelBuilder.Entity("WebApi.Entities.UserImage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("ChunkBytesLength")
                        .HasColumnType("int");

                    b.Property<byte[]>("Data")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<Guid>("FileId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("MimeType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("NextId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("TimeStamp")
                        .HasColumnType("datetimeoffset");

                    b.Property<long>("TotalBytesLength")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("UserImage");
                });
#pragma warning restore 612, 618
        }
    }
}
