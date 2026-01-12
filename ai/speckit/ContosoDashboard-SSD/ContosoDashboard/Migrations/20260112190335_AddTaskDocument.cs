using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContosoDashboard.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskDocuments",
                columns: table => new
                {
                    TaskDocumentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    AttachedByUserId = table.Column<int>(type: "int", nullable: false),
                    AttachedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskDocuments", x => x.TaskDocumentId);
                    table.ForeignKey(
                        name: "FK_TaskDocuments_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "DocumentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskDocuments_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "TaskId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskDocuments_Users_AttachedByUserId",
                        column: x => x.AttachedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskDocuments_AttachedByUserId",
                table: "TaskDocuments",
                column: "AttachedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskDocuments_DocumentId",
                table: "TaskDocuments",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskDocuments_TaskId",
                table: "TaskDocuments",
                column: "TaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskDocuments");
        }
    }
}
