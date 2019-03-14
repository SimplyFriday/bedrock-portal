using Microsoft.EntityFrameworkCore.Migrations;

namespace MinecraftWrapper.Data.Migrations
{
    public partial class MinecraftId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "NewsItem",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<string>(
                name: "MinecraftId",
                table: "AdditionalUserData",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinecraftId",
                table: "AdditionalUserData");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "NewsItem",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
