using Microsoft.EntityFrameworkCore.Migrations;

namespace MinecraftWrapper.Data.Migrations
{
    public partial class AuthorizedUserMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuthorizationKey_UserId",
                table: "AuthorizationKey");

            migrationBuilder.DropIndex(
                name: "IX_AdditionalUserData_UserId",
                table: "AdditionalUserData");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizationKey_UserId",
                table: "AuthorizationKey",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalUserData_UserId",
                table: "AdditionalUserData",
                column: "UserId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuthorizationKey_UserId",
                table: "AuthorizationKey");

            migrationBuilder.DropIndex(
                name: "IX_AdditionalUserData_UserId",
                table: "AdditionalUserData");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizationKey_UserId",
                table: "AuthorizationKey",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalUserData_UserId",
                table: "AdditionalUserData",
                column: "UserId");
        }
    }
}
