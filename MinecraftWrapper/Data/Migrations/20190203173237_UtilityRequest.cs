using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MinecraftWrapper.Data.Migrations
{
    public partial class UtilityRequest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ApplicationLog",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UtilityRequest",
                columns: table => new
                {
                    UtilityRequestId = table.Column<Guid>(nullable: false),
                    UserId = table.Column<string>(nullable: false),
                    RequestTime = table.Column<DateTime>(nullable: false),
                    UtilityRequestType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UtilityRequest", x => x.UtilityRequestId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UtilityRequest");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ApplicationLog");
        }
    }
}
