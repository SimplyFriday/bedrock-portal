using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MinecraftWrapper.Data.Migrations
{
    public partial class UserCurrency_CreatedFromTransaction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedFromTransactionId",
                table: "UserCurrency",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Command",
                table: "ScheduledTask",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserCurrency_CreatedFromTransactionId",
                table: "UserCurrency",
                column: "CreatedFromTransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserCurrency_UserCurrency_CreatedFromTransactionId",
                table: "UserCurrency",
                column: "CreatedFromTransactionId",
                principalTable: "UserCurrency",
                principalColumn: "UserCurrencyId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserCurrency_UserCurrency_CreatedFromTransactionId",
                table: "UserCurrency");

            migrationBuilder.DropIndex(
                name: "IX_UserCurrency_CreatedFromTransactionId",
                table: "UserCurrency");

            migrationBuilder.DropColumn(
                name: "CreatedFromTransactionId",
                table: "UserCurrency");

            migrationBuilder.AlterColumn<string>(
                name: "Command",
                table: "ScheduledTask",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 2000,
                oldNullable: true);
        }
    }
}
