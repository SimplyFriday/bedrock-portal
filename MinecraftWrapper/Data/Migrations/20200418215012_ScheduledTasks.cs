using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MinecraftWrapper.Data.Migrations
{
    public partial class ScheduledTasks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScheduledTask",
                columns: table => new
                {
                    ScheduledTaskId = table.Column<Guid>(nullable: false),
                    TaskName = table.Column<string>(maxLength: 50, nullable: false),
                    CronString = table.Column<string>(maxLength: 30, nullable: false),
                    Enabled = table.Column<bool>(nullable: false),
                    ScheduledTaskType = table.Column<int>(nullable: false),
                    Command = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledTask", x => x.ScheduledTaskId);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledTaskLog",
                columns: table => new
                {
                    ScheduledTaskLogId = table.Column<Guid>(nullable: false),
                    StartTime = table.Column<DateTime>(nullable: false),
                    CompletedTime = table.Column<DateTime>(nullable: true),
                    CompletionStatus = table.Column<string>(maxLength: 30, nullable: true),
                    ScheduledTaskId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledTaskLog", x => x.ScheduledTaskLogId);
                    table.ForeignKey(
                        name: "FK_ScheduledTaskLog_ScheduledTask_ScheduledTaskId",
                        column: x => x.ScheduledTaskId,
                        principalTable: "ScheduledTask",
                        principalColumn: "ScheduledTaskId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledTaskLog_ScheduledTaskId",
                table: "ScheduledTaskLog",
                column: "ScheduledTaskId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduledTaskLog");

            migrationBuilder.DropTable(
                name: "ScheduledTask");
        }
    }
}
