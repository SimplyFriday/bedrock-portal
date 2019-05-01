using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MinecraftWrapper.Data.Migrations
{
    public partial class Currency1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable (
                name: "CurrencyType",
                columns: table => new
                {
                    CurrencyTypeId = table.Column<int> ( nullable: false ),
                    Description = table.Column<string> ( nullable: false, maxLength: 50 )
                },
                constraints: table =>
                {
                    table.PrimaryKey ( "PK_CurrencyType", x => x.CurrencyTypeId );
                }
                );

            migrationBuilder.CreateTable (
                name: "CurrencyTransactionReason",
                columns: table => new
                {
                    CurrencyTransactionReasonId = table.Column<int> ( nullable: false ),
                    Description = table.Column<string> ( nullable: false, maxLength: 50 )
                },
                constraints: table =>
                {
                    table.PrimaryKey ( "PK_CurrencyTransactionReason", x => x.CurrencyTransactionReasonId );
                }
                );

            migrationBuilder.CreateTable (
                name: "UserCurrency",
                columns: table => new
                {
                    UserCurrencyId = table.Column<Guid> ( nullable: false ),
                    Amount = table.Column<decimal> ( nullable: false, type: "decimal(19,5)" ),
                    DateNoted = table.Column<DateTime> ( nullable: false ),
                    CurrencyTypeId = table.Column<int> ( nullable: false ),
                    CurrencyTransactionReasonId = table.Column<int> ( nullable: false ),
                    UserId = table.Column<string> ( nullable: false )
                },
                constraints: table =>
                {
                    table.PrimaryKey ( "PK_UserCurrency", x => x.UserCurrencyId );
                    table.ForeignKey (
                        name: "FK_UserCurrency_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade );
                    table.ForeignKey (
                        name: "FK_UserCurrency_CurrencyType_CurrencyTypeId",
                        column: x => x.CurrencyTypeId,
                        principalTable: "CurrencyType",
                        principalColumn: "CurrencyTypeId",
                        onDelete: ReferentialAction.Cascade
                        );
                    table.ForeignKey (
                        name: "FK_UserCurrency_CurrencyTransactionReason_CurrencyTransactionReasonId",
                        column: x => x.CurrencyTransactionReasonId,
                        principalTable: "CurrencyTransactionReason",
                        principalColumn: "CurrencyTransactionReasonId",
                        onDelete: ReferentialAction.Cascade
                        );
                } );

            migrationBuilder.CreateIndex (
                name: "IX_UserCurrency_UserId",
                table: "UserCurrency",
                column: "UserId" );

            migrationBuilder.Sql ( @"CREATE UNIQUE NONCLUSTERED INDEX uDiscordId
                                    ON AspNetUsers (DiscordId)
                                    WHERE DiscordId IS NOT NULL;" );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserCurrency");

            migrationBuilder.DropTable (
                name: "CurrencyType" );

            migrationBuilder.DropTable (
                name: "CurrencyTransactionReason" );

            migrationBuilder.Sql ( @"DROP INDEX uDiscordId
                                     ON dbo.AspNetUsers;" );
        }
    }
}
