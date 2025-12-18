using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemAdminAggregate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemAdmins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CorporateId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemAdmins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemAdmins_AspNetUsers_Id",
                        column: x => x.Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetUserClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "UserId" },
                values: new object[] { 1, "MedicalCenter.AdminTier", "Super", new Guid("802a729f-7f2e-d457-7b2f-b1954f70413f") });

            migrationBuilder.InsertData(
                table: "SystemAdmins",
                columns: new[] { "Id", "CorporateId", "CreatedAt", "Department", "Email", "FullName", "IsActive", "Role", "UpdatedAt" },
                values: new object[] { new Guid("802a729f-7f2e-d457-7b2f-b1954f70413f"), "SYS-ADMIN-001", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "IT", "sys.admin@medicalcenter.com", "System Administrator", true, "SystemAdmin", null });

            migrationBuilder.CreateIndex(
                name: "IX_SystemAdmins_CorporateId",
                table: "SystemAdmins",
                column: "CorporateId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemAdmins_Department",
                table: "SystemAdmins",
                column: "Department");

            migrationBuilder.CreateIndex(
                name: "IX_SystemAdmins_Email",
                table: "SystemAdmins",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemAdmins_Id_IsActive",
                table: "SystemAdmins",
                columns: new[] { "Id", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemAdmins");

            migrationBuilder.DeleteData(
                table: "AspNetUserClaims",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
