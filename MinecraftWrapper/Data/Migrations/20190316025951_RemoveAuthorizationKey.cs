using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MinecraftWrapper.Data.Migrations
{
    public partial class RemoveAuthorizationKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthorizationKey");

            migrationBuilder.AddColumn<decimal>(
                name: "xuid",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "xuid",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "AuthorizationKey",
                columns: table => new
                {
                    AuthorizationKeyId = table.Column<Guid>(nullable: false),
                    AuthorizationToken = table.Column<string>(maxLength: 16, nullable: false),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorizationKey", x => x.AuthorizationKeyId);
                    table.ForeignKey(
                        name: "FK_AuthorizationKey_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizationKey_UserId",
                table: "AuthorizationKey",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");
        }
    }
}
