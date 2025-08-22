using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetManagementSystemApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCompanyId1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Companies_CompanyId1",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_CompanyId1",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "CompanyId1",
                table: "Vehicles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId1",
                table: "Vehicles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_CompanyId1",
                table: "Vehicles",
                column: "CompanyId1",
                unique: true,
                filter: "[CompanyId1] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_Companies_CompanyId1",
                table: "Vehicles",
                column: "CompanyId1",
                principalTable: "Companies",
                principalColumn: "CompanyId");
        }
    }
}
