using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MinecraftWrapper.Data.Migrations
{
    public partial class UserCurrencyStoreItemId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StoreItemId",
                table: "UserCurrency",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserCurrency_StoreItemId",
                table: "UserCurrency",
                column: "StoreItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserCurrency_StoreItem_StoreItemId",
                table: "UserCurrency",
                column: "StoreItemId",
                principalTable: "StoreItem",
                principalColumn: "StoreItemId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserCurrency_StoreItem_StoreItemId",
                table: "UserCurrency");

            migrationBuilder.DropIndex(
                name: "IX_UserCurrency_StoreItemId",
                table: "UserCurrency");

            migrationBuilder.DropColumn(
                name: "StoreItemId",
                table: "UserCurrency");
        }
    }
}
