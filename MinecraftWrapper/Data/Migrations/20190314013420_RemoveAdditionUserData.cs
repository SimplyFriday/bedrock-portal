using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MinecraftWrapper.Data.Migrations
{
    public partial class RemoveAdditionUserData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdditionalUserData");

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiscordId",
                table: "AspNetUsers",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GamerTag",
                table: "AspNetUsers",
                maxLength: 255,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bio",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DiscordId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "GamerTag",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "AdditionalUserData",
                columns: table => new
                {
                    AdditionalUserDataId = table.Column<Guid>(nullable: false),
                    Bio = table.Column<string>(nullable: true),
                    GamerTag = table.Column<string>(maxLength: 255, nullable: true),
                    MinecraftId = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdditionalUserData", x => x.AdditionalUserDataId);
                    table.ForeignKey(
                        name: "FK_AdditionalUserData_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalUserData_UserId",
                table: "AdditionalUserData",
                column: "UserId",
                unique: true);
        }
    }
}
