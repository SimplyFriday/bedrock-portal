using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MinecraftWrapper.Data.Migrations
{
    public partial class LastMinecraftLogin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "xuid",
                table: "AspNetUsers",
                newName: "Xuid");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastMinecraftLogin",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.Sql ( @"CREATE UNIQUE NONCLUSTERED INDEX uGamerTag
                                    ON AspNetUsers (GamerTag)
                                    WHERE GamerTag IS NOT NULL;" );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastMinecraftLogin",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "Xuid",
                table: "AspNetUsers",
                newName: "xuid");

            migrationBuilder.Sql ( @"DROP INDEX uGamerTag
                                     ON dbo.AspNetUsers;" );
        }
    }
}
