using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace whot.Migrations
{
    public partial class TimeAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastActive",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastActive",
                table: "AspNetUsers");
        }
    }
}
