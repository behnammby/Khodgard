using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Khodgard.Migrations
{
    public partial class ClearMinDelayAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MinDelay",
                table: "Map",
                newName: "SyncMinDelay");

            migrationBuilder.AddColumn<int>(
                name: "ClearByAgeMinDelay",
                table: "Map",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ClearByCountMinDelay",
                table: "Map",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClearByAgeMinDelay",
                table: "Map");

            migrationBuilder.DropColumn(
                name: "ClearByCountMinDelay",
                table: "Map");

            migrationBuilder.RenameColumn(
                name: "SyncMinDelay",
                table: "Map",
                newName: "MinDelay");
        }
    }
}
