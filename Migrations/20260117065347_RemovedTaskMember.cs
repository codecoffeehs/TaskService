using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskService.Migrations
{
    /// <inheritdoc />
    public partial class RemovedTaskMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "task_member");

            migrationBuilder.AlterColumn<int>(
                name: "Repeat",
                table: "tasks",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Due",
                table: "tasks",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Repeat",
                table: "tasks",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Due",
                table: "tasks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "task_member",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvitedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_member", x => x.Id);
                    table.ForeignKey(
                        name: "FK_task_member_tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_task_member_TaskId_UserId",
                table: "task_member",
                columns: new[] { "TaskId", "UserId" },
                unique: true);
        }
    }
}
