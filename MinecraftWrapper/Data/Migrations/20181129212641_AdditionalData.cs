using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MinecraftWrapper.Data.Migrations
{
    public partial class AdditionalData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdditionalUserData",
                columns: table => new
                {
                    AdditionalUserDataId = table.Column<Guid>(nullable: false),
                    Bio = table.Column<string>(nullable: true),
                    GamerTag = table.Column<string>(nullable: true),
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
                column: "UserId");

            migrationBuilder.CreateIndex (
                name: "IX_AdditionalUserData_GamerTag",
                table: "AdditionalUserData",
                column: "GamerTag",
                unique: true );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdditionalUserData");
        }
    }
}
