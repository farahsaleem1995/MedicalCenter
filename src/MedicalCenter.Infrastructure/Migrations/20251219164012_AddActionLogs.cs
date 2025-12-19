using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddActionLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActionLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActionName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Payload = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: true),
                    ExecutedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActionLogs_ActionName_ExecutedAt",
                table: "ActionLogs",
                columns: new[] { "ActionName", "ExecutedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_ActionLogs_ExecutedAt",
                table: "ActionLogs",
                column: "ExecutedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_ActionLogs_UserId_ExecutedAt",
                table: "ActionLogs",
                columns: new[] { "UserId", "ExecutedAt" },
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActionLogs");
        }
    }
}
