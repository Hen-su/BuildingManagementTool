﻿// <auto-generated />
using System;
using BuildingManagementTool.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BuildingManagementTool.Migrations
{
    [DbContext(typeof(BuildingManagementToolDbContext))]
    [Migration("20240820133824_AddedPropertyCategoryIdToDocument")]
    partial class AddedPropertyCategoryIdToDocument
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("BuildingManagementTool.Models.Category", b =>
                {
                    b.Property<int>("CategoryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CategoryId"));

                    b.Property<string>("CategoryName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("CategoryId");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("BuildingManagementTool.Models.Document", b =>
                {
                    b.Property<int>("DocumentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("DocumentId"));

                    b.Property<string>("BlobName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ContentType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FileImageUrl")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("FileSize")
                        .HasColumnType("bigint");

                    b.Property<long>("IconUrl")
                        .HasColumnType("bigint");

                    b.Property<int>("PropertyCategoryId")
                        .HasColumnType("int");

                    b.Property<DateTime>("UploadDate")
                        .HasColumnType("datetime2");

                    b.HasKey("DocumentId");

                    b.HasIndex("PropertyCategoryId");

                    b.ToTable("Documents");
                });

            modelBuilder.Entity("BuildingManagementTool.Models.Property", b =>
                {
                    b.Property<int>("PropertyId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("PropertyId"));

                    b.Property<string>("PropertyName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("PropertyId");

                    b.ToTable("Properties");
                });

            modelBuilder.Entity("BuildingManagementTool.Models.PropertyCategory", b =>
                {
                    b.Property<int>("PropertyCategoryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("PropertyCategoryId"));

                    b.Property<int>("CategoryId")
                        .HasColumnType("int");

                    b.Property<int>("PropertyId")
                        .HasColumnType("int");

                    b.HasKey("PropertyCategoryId");

                    b.HasIndex("CategoryId");

                    b.HasIndex("PropertyId");

                    b.ToTable("PropertyCategories");
                });

            modelBuilder.Entity("BuildingManagementTool.Models.Document", b =>
                {
                    b.HasOne("BuildingManagementTool.Models.PropertyCategory", "PropertyCategory")
                        .WithMany()
                        .HasForeignKey("PropertyCategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("PropertyCategory");
                });

            modelBuilder.Entity("BuildingManagementTool.Models.PropertyCategory", b =>
                {
                    b.HasOne("BuildingManagementTool.Models.Category", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BuildingManagementTool.Models.Property", "Property")
                        .WithMany()
                        .HasForeignKey("PropertyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");

                    b.Navigation("Property");
                });
#pragma warning restore 612, 618
        }
    }
}