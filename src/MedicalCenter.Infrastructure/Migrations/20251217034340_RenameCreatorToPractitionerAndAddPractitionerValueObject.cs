using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameCreatorToPractitionerAndAddPractitionerValueObject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "MedicalRecords",
                newName: "PractitionerId");

            migrationBuilder.RenameIndex(
                name: "IX_MedicalRecords_CreatorId_IsActive",
                table: "MedicalRecords",
                newName: "IX_MedicalRecords_PractitionerId_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_MedicalRecords_CreatorId",
                table: "MedicalRecords",
                newName: "IX_MedicalRecords_PractitionerId");

            migrationBuilder.AddColumn<string>(
                name: "PractitionerEmail",
                table: "MedicalRecords",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PractitionerFullName",
                table: "MedicalRecords",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PractitionerRole",
                table: "MedicalRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalRecords_Patients_PatientId",
                table: "MedicalRecords",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicalRecords_Patients_PatientId",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "PractitionerEmail",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "PractitionerFullName",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "PractitionerRole",
                table: "MedicalRecords");

            migrationBuilder.RenameColumn(
                name: "PractitionerId",
                table: "MedicalRecords",
                newName: "CreatorId");

            migrationBuilder.RenameIndex(
                name: "IX_MedicalRecords_PractitionerId_IsActive",
                table: "MedicalRecords",
                newName: "IX_MedicalRecords_CreatorId_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_MedicalRecords_PractitionerId",
                table: "MedicalRecords",
                newName: "IX_MedicalRecords_CreatorId");
        }
    }
}
