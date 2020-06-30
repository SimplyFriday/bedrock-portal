using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MinecraftWrapper.Data.Migrations
{
    public partial class PlaytimeEvent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlaytimeEvent",
                columns: table => new
                {
                    PlaytimeEventId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EventTime = table.Column<DateTime>(nullable: false),
                    Type = table.Column<string>(nullable: true, maxLength: 10),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaytimeEvent", x => x.PlaytimeEventId);
                    table.ForeignKey(
                        name: "FK_PlaytimeEvent_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "PlaytimeEvent_UserId",
                table: "PlaytimeEvent",
                column: "UserId");
            
            migrationBuilder.Sql (
            @"CREATE NONCLUSTERED INDEX UserId_EventTime_Includes
            ON dbo.PlaytimeEvent (UserId, EventTime)
            INCLUDE (Type)"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlaytimeEvent");
        }
    }
}
