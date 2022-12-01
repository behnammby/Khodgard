using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Khodgard.Migrations
{
    public partial class AddLockedForMap : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Locked",
                table: "Map",
                newName: "LockedForSync");

            migrationBuilder.AddColumn<bool>(
                name: "LockedForClearByAge",
                table: "Map",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LockedForClearByCount",
                table: "Map",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LockedForClearByAge",
                table: "Map");

            migrationBuilder.DropColumn(
                name: "LockedForClearByCount",
                table: "Map");

            migrationBuilder.RenameColumn(
                name: "LockedForSync",
                table: "Map",
                newName: "Locked");
        }
    }
}
