using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNationalIdToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NationalId",
                table: "SystemAdmins",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NationalId",
                table: "Laboratories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NationalId",
                table: "ImagingCenters",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NationalId",
                table: "HealthcareStaff",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NationalId",
                table: "Doctors",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "SystemAdmins",
                keyColumn: "Id",
                keyValue: new Guid("802a729f-7f2e-d457-7b2f-b1954f70413f"),
                column: "NationalId",
                value: "00000000001");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NationalId",
                table: "SystemAdmins");

            migrationBuilder.DropColumn(
                name: "NationalId",
                table: "Laboratories");

            migrationBuilder.DropColumn(
                name: "NationalId",
                table: "ImagingCenters");

            migrationBuilder.DropColumn(
                name: "NationalId",
                table: "HealthcareStaff");

            migrationBuilder.DropColumn(
                name: "NationalId",
                table: "Doctors");
        }
    }
}
