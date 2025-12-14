using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedSystemAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("802a729f-7f2e-d457-7b2f-b1954f70413f"), 0, "0985cb20-fd39-4beb-5564-114092dcc5df", "sys.admin@medicalcenter.com", true, false, null, "SYS.ADMIN@MEDICALCENTER.COM", "SYS.ADMIN@MEDICALCENTER.COM", "AQAAAAIAAYagAAAAENiQk5IFLxsI3vzGppLOS4O56DOxnsRaArsRQlh+qa2jhzyB7Qtznk23hZnlhIGsPw==", null, false, "7uEf3pzBcnQoOlRcoHUrmw==", false, "sys.admin@medicalcenter.com" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { new Guid("8c50a02f-70e4-f3f9-9b62-812ad887a303"), new Guid("802a729f-7f2e-d457-7b2f-b1954f70413f") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("8c50a02f-70e4-f3f9-9b62-812ad887a303"), new Guid("802a729f-7f2e-d457-7b2f-b1954f70413f") });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("802a729f-7f2e-d457-7b2f-b1954f70413f"));
        }
    }
}
