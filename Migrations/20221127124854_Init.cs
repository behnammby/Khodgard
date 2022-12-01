using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Khodgard.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Exchange",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    ApiKey = table.Column<string>(type: "TEXT", nullable: true),
                    Secret = table.Column<string>(type: "TEXT", nullable: true),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exchange", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Market",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    BaseUnit = table.Column<string>(type: "TEXT", nullable: false),
                    QuoteUnit = table.Column<string>(type: "TEXT", nullable: false),
                    MinPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    MaxPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    AmountPrecision = table.Column<int>(type: "INTEGER", nullable: false),
                    PricePrecision = table.Column<int>(type: "INTEGER", nullable: false),
                    DepthLimit = table.Column<int>(type: "INTEGER", nullable: false),
                    ExchangeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Market", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Market_Exchange_ExchangeId",
                        column: x => x.ExchangeId,
                        principalTable: "Exchange",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Map",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    MapType = table.Column<byte>(type: "INTEGER", nullable: false),
                    SourceId = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetId = table.Column<int>(type: "INTEGER", nullable: false),
                    Ratio = table.Column<double>(type: "REAL", nullable: false),
                    MinDelay = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxAge = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxLines = table.Column<int>(type: "INTEGER", nullable: false),
                    IsRunning = table.Column<bool>(type: "INTEGER", nullable: false),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Map", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Map_Market_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Market",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Map_Market_TargetId",
                        column: x => x.TargetId,
                        principalTable: "Market",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Line",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Side = table.Column<byte>(type: "INTEGER", nullable: false),
                    Price = table.Column<decimal>(type: "TEXT", nullable: false),
                    Amount = table.Column<double>(type: "REAL", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MapId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Line", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Line_Map_MapId",
                        column: x => x.MapId,
                        principalTable: "Map",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Order",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Uid = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<double>(type: "REAL", nullable: false),
                    Price = table.Column<decimal>(type: "TEXT", nullable: false),
                    Side = table.Column<byte>(type: "INTEGER", nullable: false),
                    LineId = table.Column<int>(type: "INTEGER", nullable: false),
                    MarketId = table.Column<int>(type: "INTEGER", nullable: false),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Order_Line_LineId",
                        column: x => x.LineId,
                        principalTable: "Line",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Order_Market_MarketId",
                        column: x => x.MarketId,
                        principalTable: "Market",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Line_MapId",
                table: "Line",
                column: "MapId");

            migrationBuilder.CreateIndex(
                name: "IX_Map_SourceId",
                table: "Map",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Map_TargetId",
                table: "Map",
                column: "TargetId");

            migrationBuilder.CreateIndex(
                name: "IX_Market_ExchangeId",
                table: "Market",
                column: "ExchangeId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_LineId",
                table: "Order",
                column: "LineId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_MarketId",
                table: "Order",
                column: "MarketId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Order");

            migrationBuilder.DropTable(
                name: "Line");

            migrationBuilder.DropTable(
                name: "Map");

            migrationBuilder.DropTable(
                name: "Market");

            migrationBuilder.DropTable(
                name: "Exchange");
        }
    }
}
