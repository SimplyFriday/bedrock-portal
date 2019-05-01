using Microsoft.EntityFrameworkCore.Migrations;

namespace MinecraftWrapper.Data.Migrations
{
    public partial class StoreEffectTitle : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Effect",
                table: "StoreItem",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "StoreItem",
                maxLength: 40,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Effect",
                table: "StoreItem");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "StoreItem");
        }
    }
}
