﻿// <auto-generated />
using System;
using Khodgard.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Khodgard.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20221127124854_Init")]
    partial class Init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.11");

            modelBuilder.Entity("Khodgard.Models.Exchange", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ApiKey")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Enabled")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Secret")
                        .HasColumnType("TEXT");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Exchange");

                    b.HasDiscriminator<string>("Type").HasValue("Exchange");
                });

            modelBuilder.Entity("Khodgard.Models.Line", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<double>("Amount")
                        .HasColumnType("REAL");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT");

                    b.Property<int>("MapId")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("Price")
                        .HasColumnType("TEXT");

                    b.Property<byte>("Side")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Updated")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("MapId");

                    b.ToTable("Line");
                });

            modelBuilder.Entity("Khodgard.Models.Map", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Enabled")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsRunning")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("MapType")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MaxAge")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MaxLines")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MinDelay")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<double>("Ratio")
                        .HasColumnType("REAL");

                    b.Property<int>("SourceId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TargetId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("SourceId");

                    b.HasIndex("TargetId");

                    b.ToTable("Map");
                });

            modelBuilder.Entity("Khodgard.Models.Market", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AmountPrecision")
                        .HasColumnType("INTEGER");

                    b.Property<string>("BaseUnit")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("DepthLimit")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Enabled")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ExchangeId")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("MaxPrice")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("MinPrice")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("PricePrecision")
                        .HasColumnType("INTEGER");

                    b.Property<string>("QuoteUnit")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ExchangeId");

                    b.ToTable("Market");
                });

            modelBuilder.Entity("Khodgard.Models.Order", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<double>("Amount")
                        .HasColumnType("REAL");

                    b.Property<bool>("Enabled")
                        .HasColumnType("INTEGER");

                    b.Property<int>("LineId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MarketId")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("Price")
                        .HasColumnType("TEXT");

                    b.Property<byte>("Side")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Uid")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("LineId");

                    b.HasIndex("MarketId");

                    b.ToTable("Order");
                });

            modelBuilder.Entity("Khodgard.Exchanges.Bankdex.BankdexExchange", b =>
                {
                    b.HasBaseType("Khodgard.Models.Exchange");

                    b.HasDiscriminator().HasValue("Bankdex");
                });

            modelBuilder.Entity("Khodgard.Exchanges.Binance.BinanceExchange", b =>
                {
                    b.HasBaseType("Khodgard.Models.Exchange");

                    b.HasDiscriminator().HasValue("Binance");
                });

            modelBuilder.Entity("Khodgard.Models.Line", b =>
                {
                    b.HasOne("Khodgard.Models.Map", "Map")
                        .WithMany()
                        .HasForeignKey("MapId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Map");
                });

            modelBuilder.Entity("Khodgard.Models.Map", b =>
                {
                    b.HasOne("Khodgard.Models.Market", "Source")
                        .WithMany()
                        .HasForeignKey("SourceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Khodgard.Models.Market", "Target")
                        .WithMany()
                        .HasForeignKey("TargetId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Source");

                    b.Navigation("Target");
                });

            modelBuilder.Entity("Khodgard.Models.Market", b =>
                {
                    b.HasOne("Khodgard.Models.Exchange", "Exchange")
                        .WithMany()
                        .HasForeignKey("ExchangeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Exchange");
                });

            modelBuilder.Entity("Khodgard.Models.Order", b =>
                {
                    b.HasOne("Khodgard.Models.Line", "Line")
                        .WithMany()
                        .HasForeignKey("LineId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Khodgard.Models.Market", "Market")
                        .WithMany()
                        .HasForeignKey("MarketId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Line");

                    b.Navigation("Market");
                });
#pragma warning restore 612, 618
        }
    }
}
