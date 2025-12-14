using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MedicalCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedIdentityRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("657cb6cb-abf2-00d1-5d46-939a7b3aff5f"), "3cb3ae79-3007-7c09-cf81-9772e6eac131", "Doctor", "DOCTOR" },
                    { new Guid("7acdbd9c-9b08-5b6f-578b-b30c0ef9036c"), "f5f46d97-6fb4-8d7d-c9f0-8a4e87796955", "LabUser", "LABUSER" },
                    { new Guid("8c50a02f-70e4-f3f9-9b62-812ad887a303"), "6be7b072-8100-268f-4962-614589677748", "SystemAdmin", "SYSTEMADMIN" },
                    { new Guid("972a1201-a9dc-2127-0827-560cb7d76af8"), "4f7e2474-77b4-3507-8e73-93a66a291be9", "Patient", "PATIENT" },
                    { new Guid("c48a3633-bb7a-7d52-5130-31723217da37"), "6a015f6c-135a-ccba-933b-a048af9bdfff", "HealthcareStaff", "HEALTHCARESTAFF" },
                    { new Guid("ebdab87a-ab53-71ef-5d93-4a733f7dcd4f"), "4cca2205-1f98-817e-8af4-576abe1fe152", "ImagingUser", "IMAGINGUSER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("657cb6cb-abf2-00d1-5d46-939a7b3aff5f"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("7acdbd9c-9b08-5b6f-578b-b30c0ef9036c"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("8c50a02f-70e4-f3f9-9b62-812ad887a303"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("972a1201-a9dc-2127-0827-560cb7d76af8"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c48a3633-bb7a-7d52-5130-31723217da37"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("ebdab87a-ab53-71ef-5d93-4a733f7dcd4f"));
        }
    }
}
