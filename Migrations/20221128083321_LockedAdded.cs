using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Khodgard.Migrations
{
    public partial class LockedAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Locked",
                table: "Map",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Locked",
                table: "Map");
        }
    }
}
