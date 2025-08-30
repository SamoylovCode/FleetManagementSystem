using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetManagementSystemApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCertificateTechInspection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CertificateTechInspections",
                columns: table => new
                {
                    CertificateTechInspectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CertificateTechInspectionNum = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CertificateTechInspectionIssuedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CertificateTechInspectionIssueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CertificateTechInspectionExpDate = table.Column<DateOnly>(type: "date", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    VehicleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificateTechInspections", x => x.CertificateTechInspectionId);
                    table.ForeignKey(
                        name: "FK_CertificateTechInspections_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "VehicleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CertificateTechInspections_VehicleId",
                table: "CertificateTechInspections",
                column: "VehicleId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CertificateTechInspections");
        }
    }
}
