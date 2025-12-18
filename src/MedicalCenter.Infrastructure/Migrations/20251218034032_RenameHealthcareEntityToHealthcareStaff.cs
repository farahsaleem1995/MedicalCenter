using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameHealthcareEntityToHealthcareStaff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename the table to preserve data
            migrationBuilder.RenameTable(
                name: "HealthcareEntities",
                newName: "HealthcareStaff");

            // Rename the primary key constraint (SQL Server specific)
            migrationBuilder.Sql("EXEC sp_rename 'PK_HealthcareEntities', 'PK_HealthcareStaff'");

            // Rename the foreign key constraint (SQL Server specific)
            migrationBuilder.Sql("EXEC sp_rename 'FK_HealthcareEntities_AspNetUsers_Id', 'FK_HealthcareStaff_AspNetUsers_Id'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rename the foreign key constraint back (SQL Server specific)
            migrationBuilder.Sql("EXEC sp_rename 'FK_HealthcareStaff_AspNetUsers_Id', 'FK_HealthcareEntities_AspNetUsers_Id'");

            // Rename the primary key constraint back (SQL Server specific)
            migrationBuilder.Sql("EXEC sp_rename 'PK_HealthcareStaff', 'PK_HealthcareEntities'");

            // Rename back to original table name
            migrationBuilder.RenameTable(
                name: "HealthcareStaff",
                newName: "HealthcareEntities");
        }
    }
}
